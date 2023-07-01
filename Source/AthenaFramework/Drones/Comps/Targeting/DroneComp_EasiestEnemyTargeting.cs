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

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (!Pawn.IsHashIntervalTick(Props.searchInterval))
            {
                return;
            }

            Dictionary<Pawn, float> hostiles = PawnRegionUtility.NearbyPawnsDistances(Pawn, Props.rangeOverride ?? parent.EnemyDetectionRange, hostiles: true);

            float hitChance = -1f;
            Pawn target = null;
            HitChanceFlags hitFlags = HitChanceFlags.Posture | HitChanceFlags.Gas | HitChanceFlags.Weather | HitChanceFlags.Size | HitChanceFlags.Execution;

            for (int i = hostiles.Count - 1; i >= 0; i--)
            {
                Pawn potentialTarget = hostiles.Keys.ToList()[i];
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

            if (target != null)
            {
                parent.SetTarget(target, Props.targetPriority);
            }
        }
    }

    public class DroneCompProperties_EasiestEnemyTargeting : DroneCompProperties
    {
        public float targetPriority = 20f;
        public float? rangeOverride;
        public bool requireLineOfSight = true;

        // How often (in ticks) will the comp try to find a new target

        public int searchInterval = 15;

        public DroneCompProperties_EasiestEnemyTargeting()
        {
            compClass = typeof(DroneComp_ClosestEnemyTargeting);
        }
    }
}
