using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking.Types;
using Verse;
using Verse.AI;

namespace AthenaFramework
{
    public class CompAbilityEffect_LaunchShotgunProjectiles : CompAbilityEffect
    {
        private List<IntVec3> cachedTargetCells = new List<IntVec3>();
        private IntVec3 cachedTargetPosition;
        private IntVec3 cachedCasterPosition;

        public new CompProperties_AbilityLaunchShotgunProjectiles Props => props as CompProperties_AbilityLaunchShotgunProjectiles;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(parent.pawn.Position.x, parent.pawn.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(Props.pelletAngle * (Math.PI) / 180);

            for (int i = 0; i < Props.pelletCount - 1; i++)
            {
                float newAngle = angle - pelletAngle * (Props.pelletCount - 1) / 2 + i * pelletAngle;
                IntVec3 endPosition = (new IntVec3((int)(Math.Cos(newAngle) * parent.verb.verbProps.range), 0, (int)(Math.Sin(newAngle) * parent.verb.verbProps.range)));
                if (target.Cell.z < parent.pawn.Position.z)
                {
                    endPosition.z *= -1;
                }
                IntVec3 rangeEndPosition = endPosition + parent.pawn.Position;
                Thing newTarget = null;

                foreach (IntVec3 targetPosition in GenSight.PointsOnLineOfSight(parent.pawn.Position, rangeEndPosition).Concat(rangeEndPosition))
                {
                    if (targetPosition == parent.pawn.Position) //Prevents the parent.pawn from shooting himself
                    {
                        continue;
                    }

                    Thing targetBuilding = targetPosition.GetRoofHolderOrImpassable(parent.pawn.Map);
                    if (targetBuilding != null)
                    {
                        newTarget = targetBuilding;
                        break;
                    }

                    Thing cover = targetPosition.GetCover(parent.pawn.Map);
                    if (cover != null)
                    {
                        if (Rand.Chance(cover.BaseBlockChance()))
                        {
                            newTarget = targetBuilding;
                            break;
                        }
                    }

                    bool foundPawn = false;
                    foreach (Pawn pawnTarget in GridsUtility.GetThingList(targetPosition, parent.pawn.Map).OfType<Pawn>())
                    {
                        if (pawnTarget.Downed || pawnTarget.Dead)
                        {
                            if (Rand.Chance(Props.downedHitChance))
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
                    LaunchProjectile(new LocalTargetInfo(newTarget));
                }
                else
                {
                    LaunchProjectile(new LocalTargetInfo(rangeEndPosition));
                }
            }
        }

        public virtual bool TryFindShootLineFromTo(IntVec3 root, LocalTargetInfo targ, out ShootLine resultingLine)
        {
            if (targ.HasThing && targ.Thing.Map != parent.pawn.Map)
            {
                resultingLine = default(ShootLine);
                return false;
            }

            if (root.DistanceToSquared(targ.Cell) > parent.verb.verbProps.range || root.DistanceToSquared(targ.Cell) < parent.verb.verbProps.minRange)
            {
                resultingLine = new ShootLine(root, targ.Cell);
                return false;
            }

            if (!GenSight.LineOfSight(root, targ.Cell, parent.pawn.Map, true))
            {
                resultingLine = new ShootLine(root, targ.Cell);
                return true;
            }

            resultingLine = new ShootLine(root, targ.Cell);
            return false;
        }

        public virtual void LaunchProjectile(LocalTargetInfo target)
        {
            Pawn pawn = parent.pawn;
            ((Projectile)GenSpawn.Spawn(Props.projectileDef, pawn.Position, pawn.Map, WipeMode.Vanish)).Launch(pawn, pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget, false, null, null);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Cell != null;
        }

        public override void DrawEffectPreview(LocalTargetInfo target)
        {
            base.DrawEffectPreview(target);

            if (target == null || target.Cell == null)
            {
                return;
            }

            if (cachedCasterPosition == parent.pawn.Position && cachedTargetPosition == target.Cell && cachedTargetCells.Count > 0)
            {
                GenDraw.DrawFieldEdges(cachedTargetCells);
                return;
            }

            cachedTargetCells = new List<IntVec3>();
            cachedCasterPosition = parent.pawn.Position;
            cachedTargetPosition = target.Cell;

            GenDraw.DrawFieldEdges(GenSight.PointsOnLineOfSight(cachedCasterPosition, cachedTargetPosition).ToList());

            Log.Message(new Vector2(target.Cell.x, target.Cell.z) + "");
            Log.Message(new Vector2(parent.pawn.Position.x, parent.pawn.Position.z) + "");

            

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(parent.pawn.Position.x, parent.pawn.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(Props.pelletAngle * (Math.PI) / 180);

            for (int i = 0; i < Props.pelletCount; i++)
            {
                
                float newAngle = angle - pelletAngle * (Props.pelletCount - 1) / 2 + i * pelletAngle;
                IntVec3 endPosition = (new IntVec3((int)(Math.Cos(newAngle) * parent.verb.verbProps.range), 0, (int)(Math.Sin(newAngle) * parent.verb.verbProps.range)));
                cachedTargetCells.Add(endPosition);
                if (target.Cell.z < parent.pawn.Position.z)
                {
                    endPosition.z *= -1;
                }
                IntVec3 rangeEndPosition = endPosition + parent.pawn.Position;

                Log.Message(endPosition + " " + parent.pawn.Position);
                List<IntVec3> targetedCells = GenSight.PointsOnLineOfSight(parent.pawn.Position, rangeEndPosition).Concat(endPosition).ToList(); //GetHitTiles(parent.pawn.Position, rangeEndPosition, parent.pawn.Map);
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

                foreach (Pawn pawnTarget in GridsUtility.GetThingList(targetPosition, map).OfType<Pawn>())
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
    }

    public class CompProperties_AbilityLaunchShotgunProjectiles : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityLaunchShotgunProjectiles()
        {
            compClass = typeof(CompAbilityEffect_LaunchShotgunProjectiles);
        }

        // Projectile that you want to fire
        public ThingDef projectileDef;
        // Amount of pellets that your ability fires
        public int pelletCount;
        // Angle betweet fired pellets
        public float pelletAngle;
        // Chance for a pellet to hit a downed target when passing through a tile with one
        public float downedHitChance = 0.20f;
    }
}
