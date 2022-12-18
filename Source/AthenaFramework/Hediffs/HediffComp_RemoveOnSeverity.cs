using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_RemoveOnSeverity : HediffComp
    {
        private HediffCompProperties_RemoveOnSeverity Props => props as HediffCompProperties_RemoveOnSeverity;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (parent.Severity > Props.removeSeverity)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }
    }

    public class HediffCompProperties_RemoveOnSeverity : HediffCompProperties
    {
        public HediffCompProperties_RemoveOnSeverity()
        {
            this.compClass = typeof(HediffComp_RemoveOnSeverity);
        }

        // Hediff will remove itself upon it's severity raising above this number
        public float removeSeverity = 1f;
    }
}
