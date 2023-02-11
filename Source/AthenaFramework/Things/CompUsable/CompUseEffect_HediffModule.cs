using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class CompUseEffect_HediffModule : CompUseEffect
    {
        private new CompProperties_UseEffectHediffModule Props => props as CompProperties_UseEffectHediffModule;

        public string usedSlot;
        public HediffComp_Modular comp;
        public List<HediffComp> linkedComps = new List<HediffComp>();
        public List<Hediff> linkedHediffs = new List<Hediff>();

        public virtual List<string> Slots
        {
            get
            {
                return Props.slotIDs;
            }
        }

        public virtual float RequiredCapacity
        {
            get
            {
                return Props.requiredCapacity;
            }
        }

        public virtual List<string> ExcludeIDs
        {
            get
            {
                return Props.excludeIDs;
            }
        }

        public virtual List<HediffGraphicPackage> GetGraphics
        {
            get
            {
                return Props.additionalGraphics;
            }
        }

        public virtual bool Ejectable
        {
            get
            {
                return Props.ejectable;
            }
        }

        public virtual ModuleSlotPackage GetSlot
        {
            get
            {
                if (comp == null)
                {
                    return null;
                }

                for (int i = comp.GetAllSlots.Count - 1; i >= 0; i--)
                {
                    ModuleSlotPackage slot = comp.GetAllSlots[i];

                    if (slot.slotID == usedSlot)
                    {
                        return slot;
                    }
                }

                return null;
            }
        }

        public virtual bool Install(HediffComp_Modular holder)
        {
            if (Props.installSound != null)
            {
                Props.installSound.PlayOneShot(SoundInfo.InMap(holder.Pawn, MaintenanceType.None));
            }

            if (Props.comps != null)
            {
                for (int i = Props.comps.Count - 1; i >= 0; i--)
                {
                    HediffComp hediffComp = null;
                    try
                    {
                        hediffComp = (HediffComp)Activator.CreateInstance(Props.comps[i].compClass);
                        hediffComp.props = Props.comps[i];
                        hediffComp.parent = holder.parent;
                        holder.parent.comps.Add(hediffComp);
                        linkedComps.Add(hediffComp);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Modular HediffComp could not instantiate or initialize a HediffComp: " + ex);
                        holder.parent.comps.Remove(hediffComp);
                    }
                }
            }

            if (Props.hediffs != null)
            {
                for (int i = Props.hediffs.Count - 1; i >= 0; i--)
                {
                    Hediff hediff = HediffMaker.MakeHediff(Props.hediffs[i], holder.Pawn, holder.parent.Part);
                    holder.Pawn.health.AddHediff(hediff, holder.parent.Part);
                    linkedHediffs.Add(hediff);
                }
            }

            return true;
        }

        public virtual bool Remove(HediffComp_Modular holder)
        {
            if (!Props.ejectable)
            {
                return false;
            }

            if (Props.ejectSound != null)
            {
                Props.ejectSound.PlayOneShot(SoundInfo.InMap(holder.Pawn, MaintenanceType.None));
            }

            for (int i = linkedComps.Count - 1; i >= 0; i--)
            {
                holder.parent.comps.Remove(linkedComps[i]);
                linkedComps.RemoveAt(i); //For GC
            }

            for (int i = linkedHediffs.Count - 1; i >= 0; i--)
            {
                holder.Pawn.health.RemoveHediff(linkedHediffs[i]);
                linkedHediffs.RemoveAt(i);
            }

            return true;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref linkedHediffs, "linkedHediffs", LookMode.Reference);
        }

        public virtual void PostInit(HediffComp_Modular holder)
        {
            for (int i = Props.comps.Count - 1; i >= 0; i--)
            {
                HediffComp hediffComp = null;
                try
                {
                    hediffComp = (HediffComp)Activator.CreateInstance(Props.comps[i].compClass);
                    hediffComp.props = Props.comps[i];
                    hediffComp.parent = holder.parent;
                    holder.parent.comps.Add(hediffComp);
                    linkedComps.Add(hediffComp);
                }
                catch (Exception ex)
                {
                    Log.Error("Modular HediffComp could not instantiate or initialize a HediffComp: " + ex);
                    holder.parent.comps.Remove(hediffComp);
                }
            }
        }

        public virtual HediffStage ModifyStage(int stageIndex, HediffStage stage)
        {
            if (Props.stageOverlays == null || Props.stageOverlays.Count == 0)
            {
                return stage;
            }

            if (Props.stageOverlays.Count == 1)
            {
                return Props.stageOverlays[0].ModifyHediffStage(stage);
            }

            return Props.stageOverlays[stageIndex].ModifyHediffStage(stage);
        }

        public override void DoEffect(Pawn user)
        {
            if (comp == null || comp.parent == null || comp.Pawn == null)
            {
                comp = null;
                return;
            }

            comp.InstallModule(parent);
        }

        public override bool CanBeUsedBy(Pawn pawn, out string failReason)
        {
            if (!base.CanBeUsedBy(pawn, out failReason))
            {
                return false;
            }

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps hediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                if (hediff == null)
                {
                    continue;
                }

                for (int j = hediff.comps.Count - 1; j >= 0; j--)
                {
                    HediffComp_Modular comp = hediff.comps[j] as HediffComp_Modular;

                    if (comp != null && comp.GetOpenSlots(this).Count > 0)
                    {
                        failReason = null;
                        return true;
                    }
                }
            }

            failReason = "Cannot apply: No compatible slots availible.";
            return false;
        }
    }

    public class CompProperties_UseEffectHediffModule : CompProperties_Usable
    {
        public CompProperties_UseEffectHediffModule()
        {
            this.compClass = typeof(CompUseEffect_HediffModule);
        }

        // List of slot IDs that this module can be installed into
        public List<string> slotIDs;
        // Amount of slot capacity that this module takes
        public float requiredCapacity = 0;
        // List of IDs that this module is incompatible with
        public List<string> excludeIDs;
        // If this module can be ejected
        public bool ejectable = true;
        // Sound that is played when the module is installed
        public SoundDef installSound;
        // Sound that is played when the module is ejected
        public SoundDef ejectSound;
        // List of stage modifiers. Has to either be null, empty, a single element or equal to the parent hediff's amount of stages
        public List<StageOverlay> stageOverlays;
        // List of comps that are applied when this module is attached
        public List<HediffCompProperties> comps;
        // List of hediffs that are applied to the parent part when this module is attached
        public List<HediffDef> hediffs;
        // List of additional graphics that are applied if the parent pawn has an IRenderable that accepts hediff graphic packages linked to it
        public List<HediffGraphicPackage> additionalGraphics;
    }
}
