using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_ParentPawnTargeting : DroneComp
    {
        private DroneCompProperties_ParentPawnTargeting Props => props as DroneCompProperties_ParentPawnTargeting;

        public override (LocalTargetInfo, float) GetNewTarget()
        {
            LocalTargetInfo target = Pawn.TargetCurrentlyAimingAt;

            return (target, Props.targetPriority);
        }
    }

    public class DroneCompProperties_ParentPawnTargeting : DroneCompProperties
    {
        public float targetPriority = 10f;

        public DroneCompProperties_ParentPawnTargeting()
        {
            compClass = typeof(DroneComp_ParentPawnTargeting);
        }
    }
}
