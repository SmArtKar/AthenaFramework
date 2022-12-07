using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class ThingComp_FullTurretGraphics : ThingComp 
    { 
        public ThingComp_FullTurretGraphics() { }
    }

    public class CompProperties_FullTurretGraphics : CompProperties
    {
        public CompProperties_FullTurretGraphics()
        {
            compClass = typeof(ThingComp_FullTurretGraphics);
        }
    }
}
