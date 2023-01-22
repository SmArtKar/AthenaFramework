using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class CompUseEffect_HediffModule : CompUseEffect
    {
        private CompProperties_UseEffectHediffModule NewProps => props as CompProperties_UseEffectHediffModule;

        public List<HediffComp> linkedComps = new List<HediffComp>();
        public HediffComp_Modular comp;

        public virtual ModularHediffGroup GetData
        {
            get
            {
                return NewProps.moduleData;
            }
        }

        public virtual List<HediffCompProperties> GetComps
        {
            get
            {
                return NewProps.comps;
            }
        }

        public virtual bool Ejectable
        {
            get
            {
                return NewProps.ejectable;
            }
        }

        public virtual SoundDef EjectSound
        {
            get
            {
                return NewProps.ejectSound;
            }
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

                    if (comp != null && comp.CanInstall(this))
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

        public ModularHediffGroup moduleData;
        // If this module can be ejected
        public bool ejectable = true;
        // Sound that is played when the module is ejected
        public SoundDef ejectSound;
        // List of comps that are applied when this module is attached
        public List<HediffCompProperties> comps;
    }
}
