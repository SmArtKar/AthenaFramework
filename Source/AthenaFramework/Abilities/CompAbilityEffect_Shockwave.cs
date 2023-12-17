using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class CompAbilityEffect_Shockwave : CompAbilityEffect
    {
        private new CompProperties_AbilityShockwave Props => props as CompProperties_AbilityShockwave;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            Pawn caster = parent.pawn;
            IntVec3 position = caster.Position;
            float angle = Mathf.Atan2(-(target.Cell.z - position.z), target.Cell.x - position.x) * 57.29578f;
            GenExplosion.DoExplosion(position, caster.Map, Props.explosionRadius, Props.explosionDamageDef, caster, Props.explosionDamage, Props.explosionPenetration,
                postExplosionSpawnThingDef: Props.spawnedThingDef, postExplosionSpawnThingCount: Props.spawnThingCount, postExplosionSpawnChance: Props.spawnThingChance, postExplosionGasType: Props.explosionGasType,
                chanceToStartFire: Props.explosionFireChance, damageFalloff: Props.damageFalloff, doVisualEffects: Props.doExplosionVFX, propagationSpeed: Props.propagationSpeed,
                affectedAngle: Props.explosionAngle != null ? new FloatRange(angle - Props.explosionAngle.Value, angle + Props.explosionAngle.Value) : null);
        }

        public CompAbilityEffect_Shockwave() { }
    }

    public class CompProperties_AbilityShockwave : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityShockwave()
        {
            this.compClass = typeof(CompAbilityEffect_Shockwave);
        }

        public DamageDef explosionDamageDef;
        public int explosionDamage;
        public float explosionPenetration;
        public ThingDef spawnedThingDef;
        public int spawnThingCount;
        public float spawnThingChance;
        public GasType? explosionGasType;
        public float explosionFireChance;
        public bool damageFalloff;
        public bool doExplosionVFX;
        public float propagationSpeed;
        public float? explosionAngle;
        public float explosionRadius;
    }
}
