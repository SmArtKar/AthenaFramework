using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompDroneEquipment : ThingComp
    {
        private CompProperties_DroneEquipment Props => props as CompProperties_DroneEquipment;

        public Hediff_DroneHandler droneHediff;
        public Drone drone;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (drone == null)
            {
                drone = new Drone(pawn, Props.droneDef);
                droneHediff = drone.handlerHediff;
                return;
            }

            if (drone.pawn != null) // Happens upon loading. TYNAAAAAN
            {
                return;
            }

            drone.pawn = pawn;
            drone.SetupOnPawn();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (!Props.dronePersist)
            {
                drone.Destroy();
                drone = null;
                droneHediff = null;
                return;
            }

            drone.CleanRemove();
            droneHediff = null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            Scribe_References.Look(ref droneHediff, "droneHediff");

            if (droneHediff == null)
            {
                Scribe_Deep.Look(ref drone, "drone");
            }
        }
    }

    public class CompProperties_DroneEquipment : CompProperties
    {
        public DroneDef droneDef;
        // If the drones should be "recalled" into the item upon it getting unequipped. When set to false, drones would be instead destroyed and new ones will be spawned upon equipping
        public bool dronePersist = true;

        public CompProperties_DroneEquipment()
        {
            this.compClass = typeof(CompDroneEquipment);
        }
    }
}
