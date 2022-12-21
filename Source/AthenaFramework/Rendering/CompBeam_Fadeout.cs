using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompBeam_Fadeout : CompBeam
    {
        private CompProperties_BeamFadeout Props => props as CompProperties_BeamFadeout;

        public bool active = true;

        public override void PreDestroyBeam()
        {
            if (!active)
            {
                return;
            }

            MapComponent_AthenaRenderer renderer = Beam.Map.GetComponent<MapComponent_AthenaRenderer>();
            StaticBeamInfo beamInfo = renderer.CreateStaticBeam(Beam.firstPoint, Beam.secondPoint, Beam.def, Beam.Map);
            beamInfo.beam.fadeoutTicks = Props.fadeoutTicks;
            beamInfo.beam.ticksLeft = Props.fadeoutTicks;
            beamInfo.beam.GetComp<CompBeam_Fadeout>().active = false;
        }
    }

    public class CompProperties_BeamFadeout : CompProperties
    {
        public CompProperties_BeamFadeout()
        {
            compClass = typeof(CompBeam_Fadeout);
        }

        public int fadeoutTicks = 10;
    }
}
