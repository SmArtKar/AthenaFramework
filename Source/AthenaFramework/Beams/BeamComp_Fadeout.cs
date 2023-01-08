using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class BeamComp_Fadeout : BeamComp
    {
        private CompProperties_BeamFadeout Props => props as CompProperties_BeamFadeout;

        public bool active = true;

        public override void PreDestroy()
        {
            if (!active)
            {
                return;
            }

            Beam beam = Beam.CreateStaticBeam(Beam.firstPoint, Beam.secondPoint, Beam.def, Beam.Map);
            beam.fadeoutTicks = Props.fadeoutTicks;
            beam.ticksLeft = Props.fadeoutTicks;
            beam.GetComp<BeamComp_Fadeout>().active = false;
        }
    }

    public class CompProperties_BeamFadeout : CompProperties
    {
        public CompProperties_BeamFadeout()
        {
            compClass = typeof(BeamComp_Fadeout);
        }

        public int fadeoutTicks = 10;
    }
}
