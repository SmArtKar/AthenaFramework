using HotSwap;
using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace AthenaFramework
{
    [HotSwappable]
    public class Verb_ShootAngularShotgun : Verb_LaunchProjectile
    {
        private List<IntVec3> cachedTargetCells = new List<IntVec3>();
        private IntVec3 cachedTargetPosition;
        private IntVec3 cachedCasterPosition;

        protected override int ShotsPerBurst => verbProps.burstShotCount;

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

            AngularShotgunExtension extension = EquipmentSource.def.GetModExtension<AngularShotgunExtension>();
            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(caster.Position.x, caster.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(extension.pelletAngle * (Math.PI) / 180);

            for (int i = 0; i < extension.pelletCount - 1; i++)
            {
                float newAngle = angle - pelletAngle * (extension.pelletCount - 1) / 2 + i * pelletAngle;
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

            foreach (IntVec3 targetPosition in GenSight.PointsOnLineOfSight(startPosition, endPosition).Concat(endPosition))
            {
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

                foreach (Pawn pawnTarget in GridsUtility.GetThingList(targetPosition, map).Where((Thing x) => x is Pawn))
                {
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

        protected override bool TryCastShot()
        {
            if (EquipmentSource == null || currentTarget.Cell == null)
            {
                return false;
            }

            AngularShotgunExtension extension = EquipmentSource.def.GetModExtension<AngularShotgunExtension>();

            if (extension == null)
            {
                Log.Error(String.Format("{0} attempted to use Verb_ShootAngularShotgun without a AngularShotgunExtension mod extension", EquipmentSource.def.defName));
                return false;
            }

            bool flag = base.TryCastShot();
            if (!flag)
            {
                return false;
            }

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(currentTarget.Cell.x, currentTarget.Cell.z) - new Vector2(caster.Position.x, caster.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(extension.pelletAngle * (Math.PI) / 180);

            for (int i = 0; i < extension.pelletCount - 1; i++)
            {
                if (i == (extension.pelletCount - 1) / 2 + 1) //Skipping the central pellet that was already shot
                {
                    continue;
                }
                float newAngle = angle - pelletAngle * (extension.pelletCount - 1) / 2 + i * pelletAngle;
                IntVec3 endPosition = (new IntVec3((int)(Math.Cos(newAngle) * verbProps.range), 0, (int)(Math.Sin(newAngle) * verbProps.range)));
                if (currentTarget.Cell.z < caster.Position.z)
                {
                    endPosition.z *= -1;
                }
                IntVec3 rangeEndPosition = endPosition + caster.Position;
                Thing newTarget = null;

                foreach (IntVec3 targetPosition in GenSight.PointsOnLineOfSight(caster.Position, rangeEndPosition).Concat(rangeEndPosition))
                {
                    if (targetPosition == caster.Position) //Prevents the caster from shooting himself
                    {
                        continue;
                    }

                    Thing targetBuilding = targetPosition.GetRoofHolderOrImpassable(caster.Map);
                    if (targetBuilding != null)
                    {
                        newTarget = targetBuilding;
                        break;
                    }

                    Thing cover = targetPosition.GetCover(caster.Map);
                    if (cover != null)
                    {
                        if (Rand.Chance(cover.BaseBlockChance()))
                        {
                            newTarget = targetBuilding;
                            break;
                        }
                    }

                    bool foundPawn = false;
                    foreach (Pawn pawnTarget in GridsUtility.GetThingList(targetPosition, caster.Map).Where((Thing x) => x is Pawn))
                    {
                        if (pawnTarget.Downed || pawnTarget.Dead)
                        {
                            if (Rand.Chance(extension.downedHitChance))
                            {
                                newTarget = pawnTarget;
                                foundPawn = true;
                                break;
                            }
                        }
                        else
                        {
                            newTarget = pawnTarget;
                            foundPawn = true;
                            break;
                        }
                    }

                    if (foundPawn)
                    {
                        break;
                    }
                }


                if (newTarget != null)
                {
                    LocalTargetInfo newTargetInfo = new LocalTargetInfo(newTarget);
                    currentTarget = newTargetInfo;
                    currentDestination = newTargetInfo;
                    base.TryCastShot();
                }
                else
                {
                    if (!base.TryFindShootLineFromTo(caster.Position, new LocalTargetInfo(rangeEndPosition), out ShootLine shootLine))
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
