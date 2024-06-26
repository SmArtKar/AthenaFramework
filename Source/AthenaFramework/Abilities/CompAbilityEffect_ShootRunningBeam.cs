﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Noise;
using Verse.Sound;

namespace AthenaFramework
{
    public class CompAbilityEffect_ShootRunningBeam : CompAbilityEffect
    {
        private new CompProperties_AbilityShootRunningBeam Props => props as CompProperties_AbilityShootRunningBeam;

        public int ticksToShot = -1;
        public int shotsLeft = 0;
        public LocalTargetInfo curTarget;

        public Vector3 currentPos;
        public MoteDualAttached beamMote;
        public Effecter endEffecter;
        public Sustainer beamSustainer;
        public IntVec3 currentBeamTile;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToShot, "ticksToShot");
            Scribe_Values.Look(ref shotsLeft, "shotsLeft");
            Scribe_Values.Look(ref currentPos, "currentPos");
            Scribe_Values.Look(ref currentBeamTile, "currentBeamTile");
            Scribe_Deep.Look(ref ticksToShot, "ticksToShot");
            Scribe_References.Look(ref beamMote, "beamMote");
            Scribe_TargetInfo.Look(ref curTarget, "curTarget");
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            shotsLeft = Props.burstCount;
            curTarget = target;
            currentPos = target.Thing.DrawPos + (parent.pawn.DrawPos - target.Thing.DrawPos).normalized * 3;
            beamMote = MoteMaker.MakeInteractionOverlay(parent.VerbProperties[0].beamMoteDef, parent.pawn, new TargetInfo(currentPos.ToIntVec3(), parent.pawn.Map));
            beamMote.Maintain();

            if (endEffecter != null)
            {
                endEffecter.Cleanup();
            }

            if (parent.VerbProperties[0].soundCastBeam != null)
            {
                beamSustainer = parent.VerbProperties[0].soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(parent.pawn, MaintenanceType.PerTick));
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (shotsLeft == 0)
            {
                return;
            }

            BeamTick(curTarget);

            if (ticksToShot > 0)
            {
                ticksToShot -= 1;
                return;
            }

            if (curTarget == null || !curTarget.IsValid || !curTarget.HasThing || !curTarget.Thing.Spawned || curTarget.Thing.Destroyed)
            {
                shotsLeft = 0;
                curTarget = null;
                return;
            }

            shotsLeft--;
            ticksToShot = Props.ticksBetweenShots;
            BeamStep(curTarget);

            if (shotsLeft == 0)
            {
                curTarget = null;
            }
        }

        public virtual void BeamStep(LocalTargetInfo target)
        {
            if (target.HasThing && target.Thing.Map != parent.pawn.Map)
            {
                shotsLeft = 0;
                return;
            }

            if (beamSustainer.Ended)
            {
                beamSustainer = null;
            }

            if (beamSustainer != null)
            {
                beamSustainer.Maintain();
            }

            HitTile(currentBeamTile);
        }

        public virtual void BeamTick(LocalTargetInfo target)
        {
            if (beamMote != null)
            {
                beamMote.Maintain();
            }

            currentPos += (target.Thing.DrawPos - currentPos).normalized / Props.ticksBetweenShots;
            Vector3 currentPosVector = currentPos;
            Vector3 offsetVector = (currentPos - parent.pawn.Position.ToVector3Shifted()).Yto0();
            IntVec3 beamPos = currentPosVector.ToIntVec3();
            IntVec3 cutoffPos = GenSight.LastPointOnLineOfSight(parent.pawn.Position, beamPos, (IntVec3 x) => x.CanBeSeenOverFast(parent.pawn.Map), true);
            float normalOffset = offsetVector.MagnitudeHorizontal();

            if (cutoffPos.IsValid)
            {
                normalOffset -= (beamPos - cutoffPos).LengthHorizontal;
                currentPosVector = parent.pawn.Position.ToVector3Shifted() + offsetVector.normalized * normalOffset;
                beamPos = currentPosVector.ToIntVec3();
            }

            currentBeamTile = beamPos;

            Vector3 vector = new Vector3();

            if (Props.startOffsets != null)
            {
                if (Props.startOffsets.Count == 4)
                {
                    vector += Props.startOffsets[parent.pawn.Rotation.AsInt];
                }
                else
                {
                    vector += Props.startOffsets[0];
                }
            }

            beamMote.UpdateTargets(new TargetInfo(parent.pawn.Position, parent.pawn.Map), new TargetInfo(beamPos, parent.pawn.Map), vector, currentPosVector - beamPos.ToVector3Shifted());

            if (parent.VerbProperties[0].beamGroundFleckDef != null && Rand.Chance(parent.VerbProperties[0].beamFleckChancePerTick))
            {
                FleckMaker.Static(currentPosVector, parent.pawn.Map, parent.VerbProperties[0].beamGroundFleckDef, 1f);
            }

            if (endEffecter == null && parent.VerbProperties[0].beamEndEffecterDef != null)
            {
                endEffecter = parent.VerbProperties[0].beamEndEffecterDef.Spawn(beamPos, parent.pawn.Map, currentPosVector - beamPos.ToVector3Shifted());
            }

            if (endEffecter != null)
            {
                endEffecter.offset = currentPosVector - beamPos.ToVector3Shifted();
                endEffecter.EffectTick(new TargetInfo(beamPos, parent.pawn.Map), TargetInfo.Invalid);
                endEffecter.ticksLeft--;
            }

            if (parent.VerbProperties[0].beamLineFleckDef != null)
            {
                float normalOffsetCounter = normalOffset;
                int counter = 0;
                while (counter < normalOffsetCounter)
                {
                    if (Rand.Chance(parent.VerbProperties[0].beamLineFleckChanceCurve.Evaluate(counter / normalOffsetCounter)))
                    {
                        Vector3 fleckDistance = counter * offsetVector.normalized - offsetVector.normalized * Rand.Value + offsetVector.normalized / 2;
                        FleckMaker.Static(parent.pawn.Position.ToVector3Shifted() + fleckDistance, parent.pawn.Map, parent.VerbProperties[0].beamLineFleckDef, 1f);
                    }
                    counter++;
                }
            }

            if (beamSustainer.Ended)
            {
                beamSustainer = null;
            }

            if (beamSustainer != null)
            {
                beamSustainer.Maintain();
            }
            else if(parent.VerbProperties[0].soundCastBeam != null)
            {
                beamSustainer = parent.VerbProperties[0].soundCastBeam.TrySpawnSustainer(SoundInfo.InMap(parent.pawn, MaintenanceType.PerTick));
            }
        }

        public virtual bool CanHit(Thing thing)
        {
            return thing.Spawned && !CoverUtility.ThingCovered(thing, parent.pawn.Map);
        }

        public virtual void HitTile(IntVec3 tile)
        {
            if (parent.VerbProperties[0].beamDamageDef == null)
            {
                return;
            }

            Thing hitThing = VerbUtility.ThingsToHit(tile, parent.pawn.Map, new Func<Thing, bool>(CanHit)).RandomElementWithFallback(null);

            if (hitThing == null)
            {
                return;
            }

            BattleLogEntry_RangedImpact impactLog = new BattleLogEntry_RangedImpact(parent.pawn, hitThing, curTarget.Thing, null, null, null);
            DamageInfo damageInfo = new DamageInfo(parent.VerbProperties[0].beamDamageDef, parent.VerbProperties[0].beamDamageDef.defaultDamage, parent.VerbProperties[0].beamDamageDef.defaultArmorPenetration, (hitThing.Position - parent.pawn.Position).AngleFlat, parent.pawn, null, null, DamageInfo.SourceCategory.ThingOrUnknown, curTarget.Thing);
            hitThing.TakeDamage(damageInfo).AssociateWithLog(impactLog);

            if (hitThing.CanEverAttachFire() && Rand.Chance(parent.VerbProperties[0].beamChanceToAttachFire))
            {
                hitThing.TryAttachFire(parent.VerbProperties[0].beamFireSizeRange.RandomInRange, parent.pawn);
            }
            else if (Rand.Chance(parent.VerbProperties[0].beamChanceToStartFire))
            {
                FireUtility.TryStartFireIn(currentBeamTile, parent.pawn.Map, parent.VerbProperties[0].beamFireSizeRange.RandomInRange, parent.pawn);
            }
        }
    }

    public class CompProperties_AbilityShootRunningBeam : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityShootRunningBeam()
        {
            compClass = typeof(CompAbilityEffect_ShootRunningBeam);
        }

        public int burstCount = 1;
        public int ticksBetweenShots = 5;
        public List<Vector3> startOffsets;
    }
}
