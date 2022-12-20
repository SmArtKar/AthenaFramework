using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Unix.Native;
using RimWorld;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;
using HotSwap;
using Verse.Sound;

namespace AthenaFramework
{
    [HotSwappable]
    public class Verb_ShootRunningBeam : Verb
    {
        protected override int ShotsPerBurst
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
        Sustainer sustainer;

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

        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }

            ShootLine shootLine;
            bool flag = TryFindShootLineFromTo(caster.Position, currentTarget, out shootLine);

            IntVec3 targetTile = currentPos.Yto0().ToIntVec3();

            if (!flag)
            {
                targetTile = shootLine.Dest;
            }

            if (EquipmentSource != null)
            {
                CompChangeableProjectile compChangable = EquipmentSource.GetComp<CompChangeableProjectile>();
                if (compChangable != null)
                {
                    compChangable.Notify_ProjectileLaunched();
                }

                CompReloadable compReloadable = EquipmentSource.GetComp<CompReloadable>();
                if (compReloadable != null)
                {
                    compReloadable.UsedOnce();
                }
            }

            HitTile(targetTile);
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
                        Vector3 vector5 = counter * offsetVector.normalized - offsetVector.normalized * Rand.Value + offsetVector.normalized / 2;
                        FleckMaker.Static(caster.Position.ToVector3Shifted() + vector5, caster.Map, verbProps.beamLineFleckDef, 1f);
                    }
                    counter++;
                }
            }

            if (sustainer == null)
            {
                return;
            }

            sustainer.Maintain();
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            targeted = currentTarget.Thing;
            currentPos = targeted.DrawPos + (caster.DrawPos - targeted.DrawPos).normalized * 3;
            beamMote = MoteMaker.MakeInteractionOverlay(verbProps.beamMoteDef, caster, new TargetInfo(currentPos.ToIntVec3(), caster.Map));
            beamMote.Maintain();
        }

        public virtual void HitTile(IntVec3 tile)
        {
            return;
        }

        public virtual bool CanHit(Thing thing)
        {
            return thing.Spawned && !CoverUtility.ThingCovered(thing, caster.Map);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref currentPos, "currentPos");
            Scribe_References.Look(ref targeted, "targeted");
            Scribe_References.Look(ref beamMote, "beamMote");
        }
    }
}
