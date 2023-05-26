using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_ClosestEnemyTargeting : DroneComp
    {
        private DroneCompProperties_ClosestEnemyTargeting Props => props as DroneCompProperties_ClosestEnemyTargeting;

        public override (LocalTargetInfo, float) GetNewTarget()
        {
            Dictionary<Pawn, float> hostiles = PawnRegionUtility.NearbyPawnsDistances(Pawn, Props.rangeOverride ?? parent.EnemyDetectionRange, hostiles: true);

            float minRange = -1f;
            Pawn target = null;

            for (int i = hostiles.Count - 1; i >= 0; i--)
            {
                Pawn potentialTarget = hostiles.Keys.ToList()[i];
                float dist = hostiles[potentialTarget];

                if (dist > minRange && target != null)
                {
                    continue;
                }

                if (Props.requireLineOfSight && !GenSight.LineOfSight(parent.CurrentPosition, potentialTarget.PositionHeld, Pawn.Map, true))
                {
                    continue;
                }

                minRange = dist;
                target = potentialTarget;
            }

            return (target, Props.targetPriority);
        }
    }

    public class DroneCompProperties_ClosestEnemyTargeting : DroneCompProperties
    {
        public float targetPriority = 15f;
        public float? rangeOverride;
        public bool requireLineOfSight = true;

        public DroneCompProperties_ClosestEnemyTargeting()
        {
            compClass = typeof(DroneComp_ClosestEnemyTargeting);
        }
    }
}
