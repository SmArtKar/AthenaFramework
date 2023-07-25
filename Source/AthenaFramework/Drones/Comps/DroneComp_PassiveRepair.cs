using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_PassiveRepair : DroneComp
    {
        private DroneCompProperties_PassiveRepair Props => props as DroneCompProperties_PassiveRepair;

        public override void Tick()
        {
            base.Tick();

            if (parent.health >= parent.MaxHealth)
            {
                return;
            }

            if (Props.onlyRecalled && parent.active)
            {
                return;
            }

            if (Props.onlyOutOfCombat && parent.InCombat)
            {
                return;
            }

            parent.RepairDrone(Props.repairPerTick);
        }
    }

    public class DroneCompProperties_PassiveRepair : DroneCompProperties
    {
        // If set to true, drones will only repair themselves when recalled
        public bool onlyRecalled = false;
        // If set to true, drones will only repair themselves when the owner is out of combat
        public bool onlyOutOfCombat = false;

        public float repairPerTick = 0.05f;

        public DroneCompProperties_PassiveRepair()
        {
            compClass = typeof(DroneComp_PassiveRepair);
        }
    }
}
