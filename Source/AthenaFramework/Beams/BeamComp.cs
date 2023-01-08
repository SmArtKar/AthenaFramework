using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class BeamComp : ThingComp
    {
        public Beam Beam => parent as Beam;

        public virtual void PostTextureSetup() { }

        public virtual void PreDestroy() { }

        public virtual void MaxRangeCut() { }
    }
}
