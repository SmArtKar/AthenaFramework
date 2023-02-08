﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RimWorld;
using UnityEngine;
using UnityEngine.UI;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    [StaticConstructorOnStartup]
    public class HediffComp_Modular : HediffComp, IStageOverride, IHediffGraphicGiver
    {
        private HediffCompProperties_Modular Props => props as HediffCompProperties_Modular;

        public HediffStage cachedInput;
        public HediffStage cachedOutput;
        public bool resetCache = false; //Set to true after changing modules to force a recache
        public bool ejectableModules = true;
        public Command_Action ejectAction;
        public static readonly Texture2D cachedEjectTex = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectModule");

        public ThingOwner<ThingWithComps> moduleHolder;

        public virtual List<ModuleSlotPackage> GetAllSlots
        {
            get
            {
                return Props.slots;
            }
        }

        public virtual List<ModuleSlotPackage> GetOpenSlots(CompUseEffect_HediffModule comp)
        {
            List<ModuleSlotPackage> validSlots = new List<ModuleSlotPackage>();

            for (int i = Props.slots.Count - 1; i >= 0; i--)
            {
                ModuleSlotPackage slot = Props.slots[i];

                if (!comp.Slots.Contains(slot.slotID))
                {
                    continue;
                }

                float spaceTaken = 0;
                bool invalid = false;

                for (int j = moduleHolder.Count - 1; j >= 0; j--)
                {
                    CompUseEffect_HediffModule moduleComp = moduleHolder[j].TryGetComp<CompUseEffect_HediffModule>();

                    if (moduleComp.usedSlot == slot.slotID)
                    {
                        spaceTaken += moduleComp.RequiredCapacity;

                        if (spaceTaken + comp.RequiredCapacity > slot.capacity && slot.capacity != -1)
                        {
                            invalid = true;
                            break;
                        }
                    }

                    if (moduleComp.ExcludeIDs.Intersect(comp.ExcludeIDs).Count() > 0)
                    {
                        invalid = true;
                        break;
                    }
                }

                if (!invalid)
                {
                    validSlots.Add(slot);
                }
            }

            return validSlots;
        }

        public virtual List<HediffGraphicPackage> GetAdditionalGraphics
        {
            get
            {
                List<HediffGraphicPackage> packages = new List<HediffGraphicPackage>();

                for (int j = moduleHolder.Count - 1; j >= 0; j--)
                {
                    CompUseEffect_HediffModule moduleComp = moduleHolder[j].TryGetComp<CompUseEffect_HediffModule>();

                    if (moduleComp.GetGraphics != null)
                    {
                        packages = packages.Concat(moduleComp.GetGraphics).ToList();
                    }
                }

                return packages;
            }
        }
        
        public virtual void InstallModule(ThingWithComps thing)
        {
            CompUseEffect_HediffModule comp = thing.TryGetComp<CompUseEffect_HediffModule>();

            if (!comp.Install(this))
            {
                return;
            }

            thing.DeSpawnOrDeselect();
            moduleHolder.TryAdd(thing, false);
            resetCache = true;
            RecacheGizmo();

            if (AthenaCache.renderCache.TryGetValue(Pawn.thingIDNumber, out List<IRenderable> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    IRenderable renderable = mods[i];
                    renderable.RecacheGraphicData();
                }
            }
        }

        public virtual void RemoveModule(ThingWithComps thing)
        {
            CompUseEffect_HediffModule comp = thing.TryGetComp<CompUseEffect_HediffModule>();

            if (!comp.Remove(this))
            {
                return;
            }

            moduleHolder.TryDrop(thing, Pawn.Position, Pawn.Map, ThingPlaceMode.Near, 1, out Thing placedThing);
            resetCache = true;
            RecacheGizmo();

            if (AthenaCache.renderCache.TryGetValue(Pawn.thingIDNumber, out List<IRenderable> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    IRenderable renderable = mods[i];
                    renderable.RecacheGraphicData();
                }
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            moduleHolder = new ThingOwner<ThingWithComps>();
        }

        public virtual HediffStage GetStage(HediffStage stage)
        {
            if (stage == cachedInput && !resetCache)
            {
                return cachedOutput;
            }

            cachedInput = stage;
            resetCache = false;

            if (moduleHolder.Count == 0)
            {
                cachedOutput = stage;
                return cachedOutput;
            }

            cachedOutput = new HediffStage();
            cachedOutput.CopyValues(stage);

            for (int i = moduleHolder.Count - 1; i >= 0; i--)
            {
                ThingWithComps module = moduleHolder[i];
                CompUseEffect_HediffModule comp = module.TryGetComp<CompUseEffect_HediffModule>();

                cachedOutput = comp.ModifyStage(parent.CurStageIndex, cachedOutput);
            }

            return cachedOutput;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref moduleHolder, "moduleHolder");

            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                return;
            }
            
            if (AthenaCache.renderCache.TryGetValue(Pawn.thingIDNumber, out List<IRenderable> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    IRenderable renderable = mods[i];
                    renderable.RecacheGraphicData();
                }
            }

            if (moduleHolder == null)
            {
                moduleHolder = new ThingOwner<ThingWithComps>();
                return;
            }

            for (int i = moduleHolder.Count - 1; i >= 0; i--)
            {
                ThingWithComps module = moduleHolder[i];
                CompUseEffect_HediffModule comp = module.TryGetComp<CompUseEffect_HediffModule>();
                comp.PostInit(this);
            }
        }

        public virtual void RecacheGizmo()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();
            ejectableModules = true;

            for (int i = moduleHolder.Count - 1; i >= 0; i--)
            {
                ThingWithComps module = moduleHolder[i];
                CompUseEffect_HediffModule comp = module.TryGetComp<CompUseEffect_HediffModule>();

                if (!comp.Ejectable)
                {
                    options.Add(new FloatMenuOption("Unable to eject " + module.LabelCap + " (" + comp.GetSlot.slotName + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                    continue;
                }

                options.Add(new FloatMenuOption("Eject " + module.LabelCap + " (" + comp.GetSlot.slotName + ")", delegate () { RemoveModule(module); }));
            }

            if (options.Count == 0)
            {
                options.Add(new FloatMenuOption("No ejectable modules", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0));
                ejectableModules = false;
            }

            ejectAction = new Command_Action();
            ejectAction.defaultLabel = "Eject module";
            ejectAction.defaultDesc = "Eject a module from " + parent.LabelCap + (parent.Part != null ? " (" + parent.Part.LabelCap + ")" : "");
            ejectAction.icon = cachedEjectTex;
            ejectAction.action = delegate ()
            {
                Find.WindowStack.Add(new FloatMenu(options));
            };
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (ejectAction == null)
            {
                RecacheGizmo();
            }

            if (!ejectableModules)
            {
                yield break;
            }

            yield return ejectAction;
            yield break;
        }
    }

    public class HediffCompProperties_Modular : HediffCompProperties
    {
        public HediffCompProperties_Modular()
        {
            this.compClass = typeof(HediffComp_Modular);
        }

        // List of open slots
        public List<ModuleSlotPackage> slots = new List<ModuleSlotPackage>();
    }

    public class ModuleSlotPackage
    {
        public string slotID;
        public string slotName = "Undefined";
        // -1 means unlimited capacity
        public float capacity = -1f;
    }
}