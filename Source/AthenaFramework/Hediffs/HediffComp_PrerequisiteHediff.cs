using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class HediffComp_PrerequisiteHediff : HediffComp
    {
        private HediffCompProperties_PrerequisiteHediff Props => props as HediffCompProperties_PrerequisiteHediff;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(60) || !Props.disableIfMissing)
            {
                return;
            }

            if (Props.ValidPawn(Pawn, parent.Part))
            {
                return;
            }

            if (Props.replacementDef == null)
            {
                Log.Error("Attempted to use HediffComp_PrerequisiteHediff on " + parent.def.defName + " without specifying a replacementDef");
                return;
            }

            HediffWithComps replacer = HediffMaker.MakeHediff(Props.replacementDef, Pawn, parent.Part) as HediffWithComps;
            HediffComp_HediffRestorer restorer = replacer.TryGetComp<HediffComp_HediffRestorer>();

            if (restorer == null)
            {
                Log.Error("Attempted to use HediffComp_PrerequisiteHediff on " + parent.def.defName + " with " + Props.replacementDef.defName + " missing HediffComp_HediffRestorer");
                return;
            }

            restorer.hediffToRestore = parent;
            Pawn.health.RemoveHediff(parent);
        }

        public virtual bool ShouldReenable(Pawn pawn)
        {
            return Props.ValidPawn(pawn, parent.Part);
        }
    }

    public class HediffCompProperties_PrerequisiteHediff : HediffCompProperties
    {
        public HediffCompProperties_PrerequisiteHediff()
        {
            this.compClass = typeof(HediffComp_PrerequisiteHediff);
        }

        public List<HediffDef> prerequisites;
        // List of genes that the pawn should have for the item to be equipped
        public List<GeneDef> genePrerequisites;
        // If hediff can be implanted without prerequisites
        public bool applyWithoutPrerequisites = false;
        // Wherever prerequisite hediffs must be located on the same bodypart
        public bool samePartPrerequisites = false;
        // If hediff should be disabled if one or multiple prerequisites are missing
        public bool disableIfMissing = true;
        //Def of a hediff that will replace this hediff upon being disabled
        public HediffDef replacementDef;

        public virtual bool ValidPawn(Pawn pawn, BodyPartRecord part)
        {
            if (genePrerequisites != null && genePrerequisites.Count > 0 && pawn.genes == null)
            {
                return false;
            }

            for (int i = genePrerequisites.Count - 1; i >= 0; i--)
            {
                if (!pawn.genes.HasGene(genePrerequisites[i]))
                {
                    return false;
                }
            }

            List<HediffDef> remainingHediffs = new List<HediffDef>(prerequisites);

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];

                if (samePartPrerequisites && part != hediff.Part)
                {
                    continue;
                }

                if (remainingHediffs.Contains(hediff.def))
                {
                    remainingHediffs.Remove(hediff.def);
                }
            }

            if (remainingHediffs.Count > 0)
            {
                return false;
            }

            return true;
        }

        public virtual bool ValidSurgery(Recipe_Surgery recipe, Pawn pawn, BodyPartRecord part)
        {
            if (applyWithoutPrerequisites || prerequisites == null)
            {
                return true;
            }

            return ValidPawn(pawn, part);
        }
    }
}
