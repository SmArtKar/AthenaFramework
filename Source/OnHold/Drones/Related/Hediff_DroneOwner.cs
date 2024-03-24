using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Hediff_DroneOwner : HediffComp
    {
        private HediffCompProperties_DroneOwner Props => props as HediffCompProperties_DroneOwner;

        public Hediff_DroneHandler droneHediff;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            Drone drone = new Drone(Pawn, Props.droneDef);
            droneHediff = drone.handlerHediff;
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            droneHediff.drone.Destroy();
            droneHediff = null;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_References.Look(ref droneHediff, "droneHediff");
        }
    }

    public class HediffCompProperties_DroneOwner : HediffCompProperties
    {
        public DroneDef droneDef;

        public HediffCompProperties_DroneOwner()
        {
            this.compClass = typeof(Hediff_DroneOwner);
        }
    }
}
