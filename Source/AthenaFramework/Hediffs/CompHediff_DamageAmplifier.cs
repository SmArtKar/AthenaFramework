using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompHediff_DamageAmplifier : HediffComp
    {
        private HediffCompProperties_DamageAmplifier Props => props as HediffCompProperties_DamageAmplifier;

        public virtual float damageMultiplier
        {
            get
            {
                return Props.damageMultiplier;
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (!AthenaHediffUtility.amplifierCompsByPawn.ContainsKey(parent.pawn))
            {
                AthenaHediffUtility.amplifierCompsByPawn[parent.pawn] = new List<CompHediff_DamageAmplifier>();
            }

            AthenaHediffUtility.amplifierCompsByPawn[parent.pawn].Add(this);
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            AthenaHediffUtility.amplifierCompsByPawn[parent.pawn].Remove(this);

            if (AthenaHediffUtility.amplifierCompsByPawn[parent.pawn].Count == 0)
            {
                AthenaHediffUtility.amplifierCompsByPawn.Remove(parent.pawn);
            }
        }
    }

    public class HediffCompProperties_DamageAmplifier : HediffCompProperties
    {
        public HediffCompProperties_DamageAmplifier()
        {
            this.compClass = typeof(CompHediff_DamageAmplifier);
        }

        public float damageMultiplier = 1f;
    }
}
