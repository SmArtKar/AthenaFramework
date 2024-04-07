using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;
using Verse.Sound;
using Verse.Noise;

namespace AthenaFramework
{
    public class Verb_ShootRunningBeam : Verb
    {
        public override int ShotsPerBurst
        {
            get
            {
                return verbProps.burstShotCount;
            }
        }

        public Vector3 currentPos;
        public Thing targeted;
        public MoteDualAttached beamMote;
        public Effecter endEffecter;
        public Sustainer beamSustainer;
        public IntVec3 currentBeamTile;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentPos, "currentPos");
            Scribe_Values.Look(ref currentBeamTile, "currentBeamTile");
            Scribe_References.Look(ref beamMote, "beamMote");
            Scribe_References.Look(ref targeted, "targeted");
        }

        public override float? AimAngleOverride
        {
            get
            {
                if (state != VerbState.Bursting)
                {
                    return null;
                }

                return new float?((currentPos - caster.DrawPos).AngleFlat());
            }
        }

        public override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            if (EquipmentSource != null)
            {
                CompChangeableProjectile compChangable = EquipmentSource.GetComp<CompChangeableProjectile>();
                if (compChangable != null)
                {
                    compChangable.Notify_ProjectileLaunched();
                }

                CompApparelReloadable compReloadable = EquipmentSource.GetComp<CompApparelReloadable>();
                if (compReloadable != null)
                {
                    compReloadable.UsedOnce();
                }
            }

            if (beamSustainer != null)
            {
                if (beamSustainer.Ended)
                {
                    beamSustainer = null;
                }
                else
                {
                    beamSustainer.Maintain();
                }
            }

            HitTile(currentBeamTile);
            return true;
        }

        public override void BurstingTick()
        {
            beamMote.Maintain();
            currentPos += (targeted.DrawPos - currentPos).normalized / verbProps.ticksBetweenBurstShots;
            Vector3 currentPosVector = currentPos;
            Vector3 offsetVector = (currentPos - caster.Position.ToVector3Shifted()).Yto0();
            IntVec3 beamPos = currentPosVector.ToIntVec3();
            IntVec3 cutoffPos = GenSight.LastPointOnLineOfSight(caster.Position, beamPos, (IntVec3 x) => x.CanBeSeenOverFast(caster.Map), true);
            float normalOffset = offsetVector.MagnitudeHorizontal();

            if (cutoffPos.IsValid)
            {
                normalOffset -= (beamPos - cutoffPos).LengthHorizontal;
                currentPosVector = caster.Position.ToVector3Shifted() + offsetVector.normalized * normalOffset;
                beamPos = currentPosVector.ToIntVec3();
            }

            currentBeamTile = beamPos;
            beamMote.UpdateTargets(new TargetInfo(caster.Position, caster.Map), new TargetInfo(beamPos, caster.Map), offsetVector.normalized * verbProps.beamStartOffset, currentPosVector - beamPos.ToVector3Shifted());

            if (verbProps.beamGroundFleckDef != null && Rand.Chance(verbProps.beamFleckChancePerTick))
            {
                FleckMaker.Static(currentPosVector, caster.Map, verbProps.beamGroundFleckDef, 1f);
            }

            if (endEffecter == null && verbProps.beamEndEffecterDef != null)
            {
                endEffecter = verbProps.beamEndEffecterDef.Spawn(beamPos, caster.Map, currentPosVector - beamPos.ToVector3Shifted());
            }

            if (endEffecter != null)
            {
                endEffecter.offset = currentPosVector - beamPos.ToVector3Shifted();
                endEffecter.EffectTick(new TargetInfo(beamPos, caster.Map), TargetInfo.Invalid);
                endEffecter.ticksLeft--;
            }

            if (verbProps.beamLineFleckDef != null)
            {
                float normalOffsetCounter = normalOffset;
                int counter = 0;
                while (counter < normalOffsetCounter)
                {
                    if (Rand.Chance(verbProps.beamLineFleckChanceCurve.Evaluate(counter / normalOffsetCounter)))
                    {
                        Vector3 fleckDistance = counter * offsetVector.normalized - offsetVector.normalized * Rand.Value + offsetVector.normalized / 2;
                        FleckMaker.Static(caster.Position.ToVector3Shifted() + fleckDistance, caster.Map, verbProps.beamLineFleckDef, 1f);
                    }
                    counter++;
                }
            }

            if (beamSustainer != null)
            {
                if (beamSustainer.Ended)
                {
                    beamSustainer = null;
                }
                else
                {
                    beamSustainer.Maintain();
                }
            }
            else if(verbProps.soundCastBeam != null)
            {
                beamSustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
            }
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            targeted = currentTarget.Thing;
            currentPos = targeted.DrawPos + (caster.DrawPos - targeted.DrawPos).normalized * 3;

            Vector3 offsetVector = (currentPos - caster.Position.ToVector3Shifted()).Yto0();
            IntVec3 beamPos = currentPos.ToIntVec3();
            IntVec3 cutoffPos = GenSight.LastPointOnLineOfSight(caster.Position, beamPos, (IntVec3 x) => x.CanBeSeenOverFast(caster.Map), true);
            float normalOffset = offsetVector.MagnitudeHorizontal();

            if (cutoffPos.IsValid)
            {
                normalOffset -= (beamPos - cutoffPos).LengthHorizontal;
                currentPos = caster.Position.ToVector3Shifted() + offsetVector.normalized * normalOffset;
            }

            currentBeamTile = currentPos.ToIntVec3();

            beamMote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(currentPos.ToIntVec3(), caster.Map));
            beamMote.Maintain();
            TryCastNextBurstShot();

            if (endEffecter != null)
            {
                endEffecter.Cleanup();
            }

            if (verbProps.soundCastBeam != null)
            {
                beamSustainer = verbProps.soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(caster, MaintenanceType.PerTick));
            }
        }

        public virtual bool CanHit(Thing thing)
        {
            return thing.Spawned && !CoverUtility.ThingCovered(thing, caster.Map);
        }

        public virtual void HitTile(IntVec3 tile)
        {
            if (verbProps.beamDamageDef == null)
            {
                return;
            }

            Thing hitThing = VerbUtility.ThingsToHit(tile, caster.Map, new Func<Thing, bool>(CanHit)).RandomElementWithFallback(null);

            if (hitThing == null)
            {
                return;
            }

            BattleLogEntry_RangedImpact impactLog = new BattleLogEntry_RangedImpact(caster, hitThing, currentTarget.Thing, EquipmentSource?.def, null, null);
            DamageInfo damageInfo = new DamageInfo(verbProps.beamDamageDef, verbProps.beamDamageDef.defaultDamage, verbProps.beamDamageDef.defaultArmorPenetration, (hitThing.Position - caster.Position).AngleFlat, caster, null, EquipmentSource?.def, DamageInfo.SourceCategory.ThingOrUnknown, currentTarget.Thing);
            hitThing.TakeDamage(damageInfo).AssociateWithLog(impactLog);

            if (hitThing.CanEverAttachFire() && Rand.Chance(verbProps.beamChanceToAttachFire))
            {
                hitThing.TryAttachFire(verbProps.beamFireSizeRange.RandomInRange, Caster);
            }
            else if (Rand.Chance(verbProps.beamChanceToStartFire))
            {
                FireUtility.TryStartFireIn(currentBeamTile, caster.Map, verbProps.beamFireSizeRange.RandomInRange, Caster);
            }
        }
    }
}
