using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompBeam : ThingComp
    {
        public BeamRenderer Beam => parent as BeamRenderer;

        public virtual void PostValuesSetup() { }

        public virtual void PreDestroyBeam() { }

        public virtual void PreSelfDestroyBeam() { }

        public virtual void MaxRangeCut() { }
    }
}
