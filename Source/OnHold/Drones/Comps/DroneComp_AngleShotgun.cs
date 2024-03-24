using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_AngleShotgun : DroneComp_ProjectileLauncher
    {
        protected new DroneCompProperties_AngleShotgun Props => props as DroneCompProperties_AngleShotgun;

        public override bool Shoot(LocalTargetInfo target)
        {
            bool flag = base.Shoot(target);

            if (!flag)
            {
                return false;
            }

            IntVec3 position = parent.CurrentPosition;

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(position.x, position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(Props.pelletAngle * (Math.PI) / 180);
            float pelletAngleAmount = (Props.pelletCount - 1) / 2;

            for (int i = 0; i < Props.pelletCount; i++)
            {
                float newAngle = angle - pelletAngle * pelletAngleAmount + i * pelletAngle + Props.pelletRandomSpread * (Rand.Value * 2 - 1);

                if (i == (int)pelletAngleAmount)
                {
                    continue;
                }

                HitChanceFlags hitFlags = HitChanceFlags.Posture | HitChanceFlags.Size | HitChanceFlags.Execution | HitChanceFlags.Gas | HitChanceFlags.Weather;
                Thing newTarget = AthenaCombatUtility.GetPelletTarget(newAngle, MaxRange, position, Pawn.Map, target.Cell, out IntVec3 rangeEndPosition, hitFlags: hitFlags);

                if (newTarget != null)
                {
                    base.Shoot(newTarget);
                }
                else
                {
                    if (AthenaCombatUtility.GetShootLine(position, Pawn.Map, rangeEndPosition, out ShootLine resultingLine))
                    {
                        continue;
                    }

                    Projectile projectile = GenSpawn.Spawn(Projectile, resultingLine.Source, Pawn.Map, WipeMode.Vanish) as Projectile;
                    projectile.Launch(Pawn, parent.DrawPos, rangeEndPosition, target, ProjectileHitFlags.All, Props.preventFriendlyFire, parent.EquipmentSource, null);
                }
            }

            return true;
        }
    }

    public class DroneCompProperties_AngleShotgun : DroneCompProperties_ProjectileLauncher
    {
        // Amount of pellets that your drone fires
        public int pelletCount;
        // Angle between fired pellets
        public float pelletAngle;
        // Randomly adjusts every pellet's fire angle up to this value
        public float pelletRandomSpread = 0f;

        public DroneCompProperties_AngleShotgun()
        {
            compClass = typeof(DroneComp_AngleShotgun);
        }
    }
}
