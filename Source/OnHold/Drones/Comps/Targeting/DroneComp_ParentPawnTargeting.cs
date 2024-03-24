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

        public override bool RecacheTarget(out LocalTargetInfo newTarget, out float newPriority, float rangeOverride = -1f, List<LocalTargetInfo> blacklist = null)
        {
            base.RecacheTarget(out newTarget, out newPriority);

            if (Pawn.TargetCurrentlyAimingAt == null)
            {
                return false;
            }

            newTarget = Pawn.TargetCurrentlyAimingAt;
            newPriority = Props.targetPriority;

            return true;
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
