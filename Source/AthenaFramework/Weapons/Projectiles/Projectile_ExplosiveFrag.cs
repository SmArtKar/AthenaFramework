using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class Projectile_ExplosiveFrag : Projectile_Explosive
    {
        private static readonly HitChanceFlags flags = HitChanceFlags.Posture | HitChanceFlags.Size;
        public override void Explode()
        {
            Map map = Map;
            Destroy();

            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(PositionHeld, map), new TargetInfo(PositionHeld, map));
                effecter.Cleanup();
            }

            GenExplosion.DoExplosion(PositionHeld, map, def.projectile.explosionRadius, def.projectile.damageDef, launcher, DamageAmount, ArmorPenetration, def.projectile.soundExplode, equipmentDef, def, intendedTarget.Thing, def.projectile.postExplosionSpawnThingDef, postExplosionSpawnThingDefWater: def.projectile.postExplosionSpawnThingDefWater, postExplosionSpawnChance: def.projectile.postExplosionSpawnChance, postExplosionSpawnThingCount: def.projectile.postExplosionSpawnThingCount, postExplosionGasType: def.projectile.postExplosionGasType, preExplosionSpawnThingDef: def.projectile.preExplosionSpawnThingDef, preExplosionSpawnChance: def.projectile.preExplosionSpawnChance, preExplosionSpawnThingCount: def.projectile.preExplosionSpawnThingCount, applyDamageToExplosionCellsNeighbors: def.projectile.applyDamageToExplosionCellsNeighbors, chanceToStartFire: def.projectile.explosionChanceToStartFire, damageFalloff: def.projectile.explosionDamageFalloff, direction: origin.AngleToFlat(destination), ignoredThings: null, affectedAngle: null, doVisualEffects: true, propagationSpeed: def.projectile.damageDef.expolosionPropagationSpeed, excludeRadius: 0f, doSoundEffects: true, screenShakeFactor: def.projectile.screenShakeFactor);

            FragGrenadeExtension extension = def.GetModExtension<FragGrenadeExtension>();

            float angle = (float)(Rand.Value * 2 * (Math.PI));
            float pelletAngle = (float)(extension.projAngle * (Math.PI) / 180);
            float pelletAngleAmount = (extension.projCount - 1) / 2;

            for (int i = 0; i < extension.projCount; i++)
            {
                float newAngle = angle - pelletAngle * pelletAngleAmount + i * pelletAngle + extension.randomSpread * (Rand.Value * 2 - 1);

                if (i == (int)pelletAngleAmount)
                {
                    continue;
                }

                LocalTargetInfo newTarget = AthenaCombatUtility.GetPelletTarget(newAngle, extension.range, PositionHeld, map, IntVec3.Zero, out IntVec3 rangeEndPosition, hitFlags: flags);

                if (newTarget == null)
                {
                    newTarget = rangeEndPosition;
                }

                Projectile projectile = GenSpawn.Spawn(extension.projectileDef, PositionHeld, map, WipeMode.Vanish) as Projectile;
                projectile.Launch(this, DrawPos, newTarget, rangeEndPosition, ProjectileHitFlags.All);
            }
        }
    }

    public class FragGrenadeExtension : DefModExtension
    {
        // Amount of projectiles that the grenade fires upon exploding
        public int projCount;
        // Angle between fired projectiles
        public float projAngle;
        // Maximum range for projectiles
        public float range;
        // Randomly adjusts every projectile fire angle up to this value
        public float randomSpread = 0f;
        // Projectiles fired by this grenade
        public ThingDef projectileDef;
    }
}
