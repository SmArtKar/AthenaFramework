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

        public override bool RecacheTarget(out LocalTargetInfo newTarget, out float newPriority, float rangeOverride = -1f, List<LocalTargetInfo> blacklist = null)
        {
            base.RecacheTarget(out newTarget, out newPriority);

            Dictionary<Pawn, float> hostiles = PawnRegionUtility.NearbyPawnsDistances(parent.CurrentPosition, Pawn.Map, rangeOverride != -1f ? rangeOverride : Props.rangeOverride ?? parent.EnemyDetectionRange, TraverseParms.For(Pawn), givenPawn: Pawn, hostiles: true);

            float minRange = -1f;
            Pawn target = null;

            List<Pawn> pawnHostiles = hostiles.Keys.ToList();

            for (int i = hostiles.Count - 1; i >= 0; i--)
            {
                Pawn potentialTarget = pawnHostiles[i];

                if (blacklist != null && blacklist.Contains(potentialTarget))
                {
                    continue;
                }

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

            if (target == null)
            {
                return false;
            }

            newTarget = target;
            newPriority = Props.targetPriority;

            return true;
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
