﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using static UnityEngine.GraphicsBuffer;

namespace AthenaFramework
{
    public class Verb_ShootAngularShotgun : Verb_LaunchProjectile
    {
        public List<IntVec3> cachedTargetCells = new List<IntVec3>();
        public IntVec3 cachedTargetPosition;
        public IntVec3 cachedCasterPosition;

        public override int ShotsPerBurst => verbProps.burstShotCount;

        public AngularShotgunExtension ShotgunExtension
        {
            get
            {
                if (HediffSource != null)
                {
                    return HediffSource.def.GetModExtension<AngularShotgunExtension>();
                }

                return EquipmentSource.def.GetModExtension<AngularShotgunExtension>();
            }
        }

        public override void WarmupComplete()
        {
            base.WarmupComplete();
            Pawn pawn = currentTarget.Thing as Pawn;
            if (pawn != null && !pawn.Downed && !pawn.IsColonyMech && CasterIsPawn && CasterPawn.skills != null)
            {
                float friendlyModifier = (pawn.HostileTo(caster) ? 170f : 20f);
                float cycleTimeModifier = verbProps.AdjustedFullCycleTime(this, CasterPawn);
                CasterPawn.skills.Learn(SkillDefOf.Shooting, friendlyModifier * cycleTimeModifier, false);
            }
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);

            if (target == null || target.Cell == null)
            {
                return;
            }

            if (cachedCasterPosition == caster.Position && cachedTargetPosition == target.Cell)
            {
                GenDraw.DrawFieldEdges(cachedTargetCells);
                return;
            }

            cachedTargetCells = new List<IntVec3>();
            cachedCasterPosition = caster.Position;
            cachedTargetPosition = target.Cell;

            AngularShotgunExtension extension = ShotgunExtension;
            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(caster.Position.x, caster.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(extension.pelletAngle * (Math.PI) / 180);
            float pelletAngleAmount = (extension.pelletCount - 1) / 2;

            for (int i = 0; i < extension.pelletCount; i++)
            {
                float newAngle = angle - pelletAngle * pelletAngleAmount + i * pelletAngle;

                if (extension.pelletCount % 2 == 0 && i == (int)pelletAngleAmount)
                {
                    newAngle = angle;
                }

                IntVec3 endPosition = (new IntVec3((int)(Math.Cos(newAngle) * verbProps.range), 0, (int)(Math.Sin(newAngle) * verbProps.range)));
                if (target.Cell.z < caster.Position.z)
                {
                    endPosition.z *= -1;
                }
                IntVec3 rangeEndPosition = endPosition + caster.Position;

                List<IntVec3> targetedCells = GetHitTiles(caster.Position, rangeEndPosition, caster.Map);
                cachedTargetCells = cachedTargetCells.Concat(targetedCells).ToList();
            }

            GenDraw.DrawFieldEdges(cachedTargetCells);
        }

        public List<IntVec3> GetHitTiles(IntVec3 startPosition, IntVec3 endPosition, Map map)
        {
            List<IntVec3> cellList = new List<IntVec3>();

            List<IntVec3> points = GenSight.PointsOnLineOfSight(startPosition, endPosition).Concat(endPosition).ToList();
            for (int i = 0; i < points.Count; i++)
            {
                IntVec3 targetPosition = points[i];

                if (!targetPosition.IsValid) 
                { 
                    continue; 
                }

                if (targetPosition == startPosition)
                {
                    cellList.Add(targetPosition);
                    continue;
                }

                Thing targetBuilding = targetPosition.GetRoofHolderOrImpassable(map);
                if (targetBuilding != null)
                {
                    break;
                }

                cellList.Add(targetPosition);
                bool foundPawn = false;

                List<Thing> thingList = GridsUtility.GetThingList(targetPosition, map);
                for (int j = thingList.Count - 1; j >= 0; j--)
                {
                    Pawn pawnTarget = thingList[j] as Pawn;

                    if (pawnTarget == null)
                    {
                        continue;
                    }

                    if (!pawnTarget.Downed && !pawnTarget.Dead)
                    {
                        foundPawn = true;
                        break;
                    }
                }

                if (foundPawn)
                {
                    break;
                }
            }

            return cellList;
        }

        public override bool TryCastShot()
        {
            if (EquipmentSource == null || currentTarget.Cell == null)
            {
                return false;
            }

            AngularShotgunExtension extension = ShotgunExtension;

            if (extension == null)
            {
                Log.Error(String.Format("{0} attempted to use Verb_ShootAngularShotgun without an AngularShotgunExtension mod extension", HediffSource == null ? EquipmentSource.def.defName : HediffSource.def.defName));
                return false;
            }

            bool flag = base.TryCastShot();
            if (!flag)
            {
                return false;
            }

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(currentTarget.Cell.x, currentTarget.Cell.z) - new Vector2(caster.Position.x, caster.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(extension.pelletAngle * (Math.PI) / 180);
            float pelletAngleAmount = (extension.pelletCount - 1) / 2;

            for (int i = 0; i < extension.pelletCount; i++)
            {
                float newAngle = angle - pelletAngle * pelletAngleAmount + i * pelletAngle + extension.pelletRandomSpread * (Rand.Value * 2 - 1);

                if (i == (int)pelletAngleAmount)
                {
                    continue;
                }

                LocalTargetInfo cachedTarget = currentTarget;

                Thing newTarget = AthenaCombatUtility.GetPelletTarget(newAngle, verbProps.range, caster.Position, caster.Map, currentTarget.Cell, out IntVec3 rangeEndPosition, caster: caster, verb: this);

                if (newTarget != null)
                {
                    LocalTargetInfo newTargetInfo = new LocalTargetInfo(newTarget);
                    currentTarget = newTargetInfo;
                    currentDestination = newTargetInfo;
                    base.TryCastShot();
                    currentTarget = cachedTarget;
                }
                else
                {
                    if (!TryFindShootLineFromTo(caster.Position, new LocalTargetInfo(rangeEndPosition), out ShootLine shootLine))
                    {
                        continue;
                    }

                    Projectile projectile = GenSpawn.Spawn(Projectile, shootLine.Source, caster.Map, WipeMode.Vanish) as Projectile;
                    projectile.Launch(caster, caster.DrawPos, rangeEndPosition, currentTarget, ProjectileHitFlags.All, preventFriendlyFire, EquipmentSource, null);
                }
            }

            return true;
        }
    }
}
