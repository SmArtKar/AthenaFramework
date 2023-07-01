using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class ProjectileComp_Scattershot : ProjectileComp
    {
        private CompProperties_ProjectileScattershot Props => props as CompProperties_ProjectileScattershot;

        public override void CompTick()
        {
            base.CompTick();

            if ((Projectile.ExactPosition - Projectile.origin).sqrMagnitude < Props.scatterRange * Props.scatterRange)
            {
                return;
            }

            AngularShotgunExtension extension = Projectile.def.GetModExtension<AngularShotgunExtension>();

            if (extension == null)
            {
                Log.Error(String.Format("{0} attempted to use ProjectileComp_Scattershot without an AngularShotgunExtension mod extension", Projectile.def.defName));
                return;
            }

            LocalTargetInfo target = Projectile.usedTarget;

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(Projectile.Position.x, Projectile.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(extension.pelletAngle * (Math.PI) / 180);
            float pelletAngleAmount = (extension.pelletCount - 1) / 2;

            for (int i = 0; i < extension.pelletCount; i++)
            {
                float newAngle = angle - pelletAngle * pelletAngleAmount + i * pelletAngle + extension.pelletRandomSpread * (Rand.Value * 2 - 1);

                if (i == (int)pelletAngleAmount)
                {
                    continue;
                }

                HitChanceFlags hitFlags = HitChanceFlags.Posture | HitChanceFlags.Size | HitChanceFlags.Execution;

                Thing newTarget = AthenaCombatUtility.GetPelletTarget(newAngle, Props.pelletRange, Projectile.Position, Projectile.Map, target.Cell, out IntVec3 rangeEndPosition, caster: Projectile, hitFlags: hitFlags);

                if (newTarget != null)
                {
                    Projectile projectile = GenSpawn.Spawn(Projectile, Projectile.Position, Projectile.Map, WipeMode.Vanish) as Projectile;
                    projectile.Launch(Projectile, Projectile.DrawPos, newTarget, newTarget, ProjectileHitFlags.All);
                }
                else
                {
                    Projectile projectile = GenSpawn.Spawn(Projectile, Projectile.Position, Projectile.Map, WipeMode.Vanish) as Projectile;
                    projectile.Launch(Projectile, Projectile.DrawPos, rangeEndPosition, rangeEndPosition, ProjectileHitFlags.All);
                }
            }

            Projectile.Destroy();
        }
    }

    public class CompProperties_ProjectileScattershot : CompProperties
    {
        public CompProperties_ProjectileScattershot()
        {
            this.compClass = typeof(ProjectileComp_Scattershot);
        }

        public float scatterRange = 4.9f;
        public float pelletRange = 7.9f;
    }
}
