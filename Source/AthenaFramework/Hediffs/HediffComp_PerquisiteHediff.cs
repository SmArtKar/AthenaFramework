using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class HediffComp_PerquisiteHediff : HediffComp
    {
        private HediffCompProperties_PerquisiteHediff Props => props as HediffCompProperties_PerquisiteHediff;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(60) || !Props.disableIfMissing)
            {
                return;
            }

            List<HediffDef> remainingDefs = new List<HediffDef>(Props.perquisites);

            for (int i = Pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = Pawn.health.hediffSet.hediffs[i];

                if (Props.samePartPerquisites && parent.Part != hediff.Part)
                {
                    continue;
                }

                if (remainingDefs.Contains(hediff.def))
                {
                    remainingDefs.Remove(hediff.def);
                }
            }

            if (remainingDefs.Count == 0)
            {
                return;
            }

            if (Props.replacementDef == null)
            {
                Log.Error("Attempted to use HediffComp_PerquisiteHediff on " + parent.def.defName + " without specifying a replacementDef");
                return;
            }

            HediffWithComps replacer = HediffMaker.MakeHediff(Props.replacementDef, Pawn, parent.Part) as HediffWithComps;
            HediffComp_HediffRestorer restorer = replacer.TryGetComp<HediffComp_HediffRestorer>();

            if (restorer == null)
            {
                Log.Error("Attempted to use HediffComp_PerquisiteHediff on " + parent.def.defName + " with " + Props.replacementDef.defName + " missing HediffComp_HediffRestorer");
                return;
            }

            restorer.hediffToRestore = parent;
            Pawn.health.RemoveHediff(parent);
        }

        public virtual bool ShouldReenable(Pawn pawn)
        {
            List<HediffDef> remainingDefs = new List<HediffDef>(Props.perquisites);

            for (int i = Pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = Pawn.health.hediffSet.hediffs[i];

                if (Props.samePartPerquisites && parent.Part != hediff.Part)
                {
                    continue;
                }

                if (remainingDefs.Contains(hediff.def))
                {
                    remainingDefs.Remove(hediff.def);
                }
            }

            if (remainingDefs.Count == 0)
            {
                return true;
            }

            return false;
        }
    }

    public class HediffCompProperties_PerquisiteHediff : HediffCompProperties
    {
        public HediffCompProperties_PerquisiteHediff()
        {
            this.compClass = typeof(HediffComp_PerquisiteHediff);
        }

        public List<HediffDef> perquisites;
        // If hediff can be implanted without perquisites
        public bool applyWithoutPerquisites = false;
        // Wherever perquisite hediffs must be located on the same bodypart
        public bool samePartPerquisites = false;
        // If hediff should be disabled if one or multiple perquisites are missing
        public bool disableIfMissing = true;
        //Def of a hediff that will replace this hediff upon being disabled
        public HediffDef replacementDef;

        public virtual bool ValidSurgery(Recipe_Surgery recipe, Pawn pawn, BodyPartRecord part)
        {
            if (applyWithoutPerquisites || perquisites == null)
            {
                return true;
            }

            List<HediffDef> remainingDefs = new List<HediffDef>(perquisites);

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];
                
                if (samePartPerquisites && part != hediff.Part)
                {
                    continue;
                }

                if (remainingDefs.Contains(hediff.def))
                {
                    remainingDefs.Remove(hediff.def);
                }
            }

            return remainingDefs.Count == 0;
        }
    }
}
