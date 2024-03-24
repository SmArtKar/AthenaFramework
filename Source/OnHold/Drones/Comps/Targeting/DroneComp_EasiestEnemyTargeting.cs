using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_EasiestEnemyTargeting : DroneComp
    {
        private DroneCompProperties_EasiestEnemyTargeting Props => props as DroneCompProperties_EasiestEnemyTargeting;

        public override bool RecacheTarget(out LocalTargetInfo newTarget, out float newPriority, float rangeOverride = -1f, List<LocalTargetInfo> blacklist = null)
        {
            base.RecacheTarget(out newTarget, out newPriority);

            Dictionary<Pawn, float> hostiles = PawnRegionUtility.NearbyPawnsDistances(parent.CurrentPosition, Pawn.Map, rangeOverride != -1f ? rangeOverride : Props.rangeOverride ?? parent.EnemyDetectionRange, TraverseParms.For(Pawn), givenPawn: Pawn, hostiles: true);

            float hitChance = -1f;
            Pawn target = null;
            HitChanceFlags hitFlags = HitChanceFlags.Posture | HitChanceFlags.Gas | HitChanceFlags.Weather | HitChanceFlags.Size | HitChanceFlags.Execution;

            List<Pawn> pawnHostiles = hostiles.Keys.ToList();

            for (int i = hostiles.Count - 1; i >= 0; i--)
            {
                Pawn potentialTarget = pawnHostiles[i];

                if (blacklist.Contains(potentialTarget))
                {
                    continue;
                }

                float newHitChance = AthenaCombatUtility.GetRangedHitChance(parent.CurrentPosition, potentialTarget, hitFlags);
                newHitChance *= parent.GetRangedHitChance((float)Math.Sqrt(hostiles[potentialTarget]));

                if (hitChance > newHitChance && target != null)
                {
                    continue;
                }

                if (Props.requireLineOfSight && !GenSight.LineOfSight(parent.CurrentPosition, potentialTarget.PositionHeld, Pawn.Map, true))
                {
                    continue;
                }

                hitChance = newHitChance;
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

    public class DroneCompProperties_EasiestEnemyTargeting : DroneCompProperties
    {
        public float targetPriority = 20f;
        public float? rangeOverride;
        public bool requireLineOfSight = true;

        public DroneCompProperties_EasiestEnemyTargeting()
        {
            compClass = typeof(DroneComp_ClosestEnemyTargeting);
        }
    }
}
