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

        public override (LocalTargetInfo, float) GetNewTarget()
        {
            Dictionary<Pawn, float> hostiles = PawnRegionUtility.NearbyPawnsDistances(Pawn, Props.rangeOverride ?? parent.EnemyDetectionRange, hostiles: true);

            float hitChance = -1f;
            Pawn target = null;
            HitChanceFlags hitFlags = HitChanceFlags.Posture | HitChanceFlags.Gas | HitChanceFlags.Weather | HitChanceFlags.Size | HitChanceFlags.Execution;

            for (int i = hostiles.Count - 1; i >= 0; i--)
            {
                Pawn potentialTarget = hostiles.Keys.ToList()[i];
                float newHitChance = AthenaCombatUtility.GetHitChance(parent.CurrentPosition, potentialTarget, hitFlags);
                newHitChance *= parent.GetHitChance((float)Math.Sqrt(hostiles[potentialTarget]));

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

            return (target, Props.targetPriority);
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
