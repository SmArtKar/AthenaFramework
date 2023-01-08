using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class Comp_BeamProjectile : ThingComp
    {
        public CompProperties_BeamProjectile Props => props as CompProperties_BeamProjectile;

        public Beam beam;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref beam, "beam");
        }
    }

    public class CompProperties_BeamProjectile : CompProperties
    {
        public CompProperties_BeamProjectile()
        {
            compClass = typeof(Comp_BeamProjectile);
        }

        public ThingDef beamDef;
    }
}
