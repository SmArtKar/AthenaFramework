using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class Tool_Shockwave : AdvancedTool
    {
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

        public override void DamageModification(Verb verb, ref DamageInfo dinfo, LocalTargetInfo target, Pawn caster, out IEnumerator<DamageInfo> additionalDamage)
        {
            IntVec3 position = caster.Position;
            float angle = Mathf.Atan2(-(target.Cell.z - position.z), target.Cell.x - position.x) * 57.29578f;
            base.DamageModification(verb, ref dinfo, target, caster, out additionalDamage);
            GenExplosion.DoExplosion(position, caster.Map, explosionRadius, explosionDamageDef, caster, explosionDamage, explosionPenetration, weapon: verb.EquipmentSource?.def, 
                postExplosionSpawnThingDef: spawnedThingDef, postExplosionSpawnThingCount: spawnThingCount, postExplosionSpawnChance: spawnThingChance, postExplosionGasType: explosionGasType,
                chanceToStartFire: explosionFireChance, damageFalloff: damageFalloff, doVisualEffects: doExplosionVFX, propagationSpeed: propagationSpeed, 
                affectedAngle: explosionAngle != null ? new FloatRange(angle - explosionAngle.Value, angle + explosionAngle.Value) : null);
        }
    }
}
