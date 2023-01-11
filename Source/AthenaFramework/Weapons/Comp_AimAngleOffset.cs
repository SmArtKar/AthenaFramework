using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Comp_AimAngleOffset : ThingComp
    {
        public CompProperties_AimAngleOffset Props => props as CompProperties_AimAngleOffset;
    }

    public class CompProperties_AimAngleOffset : CompProperties
    {
        public CompProperties_AimAngleOffset()
        {
            this.compClass = typeof(Comp_AimAngleOffset);
        }

        public float angleOffset = 0f;
    }
}
