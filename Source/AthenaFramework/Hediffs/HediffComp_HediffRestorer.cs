using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_HediffRestorer : HediffComp
    {
        public HediffWithComps hediffToRestore;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (hediffToRestore == null || Pawn == null || parent.Part == null)
            {
                return;
            }

            Pawn.health.AddHediff(hediffToRestore, parent.Part, null, null);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref hediffToRestore, "hediffToRestore");
        }
    }

    public class HediffCompProperties_HediffRestorer : HediffCompProperties
    {
        public HediffCompProperties_HediffRestorer()
        {
            this.compClass = typeof(HediffComp_HediffRestorer);
        }
    }
}
