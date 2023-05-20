using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    [StaticConstructorOnStartup]
    public class CompModular : ThingComp, IEquippableGraphicGiver
    {
        private CompProperties_Modular Props => props as CompProperties_Modular;

        public bool ejectableModules = true;
        public Command_Action ejectAction;
        public static readonly Texture2D cachedEjectTex = ContentFinder<Texture2D>.Get("UI/Gizmos/EjectModule");

        public ThingOwner<ThingWithComps> moduleHolder;
        public Dictionary<StatDef, float> gearStatOffsets = new Dictionary<StatDef, float>();
        public Dictionary<StatDef, float> statOffsets = new Dictionary<StatDef, float>();
        public Dictionary<StatDef, float> statFactors = new Dictionary<StatDef, float>();

        public virtual Pawn Holder
        {
            get
            {
                if (parent is Apparel)
                {
                    return (parent as Apparel).Wearer;
                }

                CompEquippable comp = parent.TryGetComp<CompEquippable>();

                if (comp != null)
                {
                    return comp.Holder;
                }

                return null;
            }
        }
        public virtual List<ModuleSlotPackage> GetAllSlots
        {
            get
            {
                return Props.slots;
            }
        }

        public virtual List<ModuleSlotPackage> GetOpenSlots(CompUseEffect_Module comp)
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
                    CompUseEffect_Module moduleComp = moduleHolder[j].TryGetComp<CompUseEffect_Module>();

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

        public virtual List<ApparelGraphicPackage> GetAdditionalGraphics
        {
            get
            {
                List<ApparelGraphicPackage> packages = new List<ApparelGraphicPackage>();

                for (int j = moduleHolder.Count - 1; j >= 0; j--)
                {
                    CompUseEffect_Module moduleComp = moduleHolder[j].TryGetComp<CompUseEffect_Module>();

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
            CompUseEffect_Module comp = thing.TryGetComp<CompUseEffect_Module>();

            if (!comp.Install(this))
            {
                return;
            }

            thing.DeSpawnOrDeselect();
            moduleHolder.TryAdd(thing, false);
            RecacheGizmo();

            if (AthenaCache.renderCache.TryGetValue(parent.thingIDNumber, out List<IRenderable> mods))
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
            CompUseEffect_Module comp = thing.TryGetComp<CompUseEffect_Module>();
            if (!comp.Remove(this))
            {
                return;
            }

            moduleHolder.TryDrop(thing, parent.Position, parent.Map, ThingPlaceMode.Near, 1, out Thing placedThing);
            RecacheGizmo();

            if (AthenaCache.renderCache.TryGetValue(parent.thingIDNumber, out List<IRenderable> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    IRenderable renderable = mods[i];
                    renderable.RecacheGraphicData();
                }
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            moduleHolder = new ThingOwner<ThingWithComps>();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref moduleHolder, "moduleHolder");

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
            {
                return;
            }

            if (AthenaCache.renderCache.TryGetValue(parent.thingIDNumber, out List<IRenderable> mods))
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
                CompUseEffect_Module comp = module.TryGetComp<CompUseEffect_Module>();

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
                CompUseEffect_Module comp = module.TryGetComp<CompUseEffect_Module>();

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
            ejectAction.defaultDesc = "Eject a module from " + parent.LabelCap;
            ejectAction.icon = cachedEjectTex;
            ejectAction.action = delegate ()
            {
                Find.WindowStack.Add(new FloatMenu(options));
            };
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (!Props.dropModulesOnDestoy)
            {
                return;
            }

            while (moduleHolder.Count > 0)
            {
                RemoveModule(moduleHolder[0]);
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
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

    public class CompProperties_Modular : CompProperties
    {
        public CompProperties_Modular()
        {
            this.compClass = typeof(CompModular);
        }

        // If all modules should be dropped upon the item getting destroyed
        public bool dropModulesOnDestoy = true;

        // List of open slots
        public List<ModuleSlotPackage> slots = new List<ModuleSlotPackage>();
    }
}
