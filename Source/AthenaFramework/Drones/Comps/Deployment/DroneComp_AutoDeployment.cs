using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_AutoDeployment : DroneComp
    {
        private DroneCompProperties_AutoDeployment Props => props as DroneCompProperties_AutoDeployment;

        public override void Tick()
        {
            base.Tick();

            if (!Pawn.IsHashIntervalTick(15))
            {
                return;
            }

            bool shouldBeDeployed = false;

            if (Props.deployWhenDrafted && Pawn.Drafted)
            {
                shouldBeDeployed = true;
            }
            else if (Props.deployInCombat && parent.InCombat)
            {
                shouldBeDeployed = true;
            }

            if (parent.active)
            {
                if (!shouldBeDeployed)
                {
                    parent.Recall();
                }
            }
            else
            {
                if (shouldBeDeployed)
                {
                    parent.Deploy();
                }
            }
        }
    }

    public class DroneCompProperties_AutoDeployment : DroneCompProperties
    {
        public bool deployWhenDrafted = true;
        public bool deployInCombat = true;

        public DroneCompProperties_AutoDeployment()
        {
            compClass = typeof(DroneComp_AutoDeployment);
        }
    }
}
