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
        public List<IntVec3> cachedTargetCells = new List<IntVec3>();
        public IntVec3 cachedTargetPosition;
        public IntVec3 cachedCasterPosition;

        public new CompProperties_AbilityLaunchShotgunProjectiles Props => props as CompProperties_AbilityLaunchShotgunProjectiles;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            float angle = (float)Math.Acos(Vector2.Dot((new Vector2(target.Cell.x, target.Cell.z) - new Vector2(parent.pawn.Position.x, parent.pawn.Position.z)).normalized, new Vector2(1, 0)));
            float pelletAngle = (float)(Props.pelletAngle * (Math.PI) / 180);
            float pelletAngleAmount = (Props.pelletCount - 1) / 2;

            for (int i = 0; i < Props.pelletCount; i++)
            {
                float newAngle = angle - pelletAngle * pelletAngleAmount + i * pelletAngle + Props.pelletRandomSpread * (Rand.Value * 2 - 1);

                Thing newTarget = AthenaCombatUtility.GetPelletTarget(newAngle, parent.verb.verbProps.range, parent.pawn.Position, parent.pawn.Map, target.Cell, out IntVec3 rangeEndPosition, caster: parent.pawn, verb: parent.verb);

                if (newTarget != null)
                {
                    LaunchProjectile(new LocalTargetInfo(newTarget));
                }
                else
                {
                    if (!TryFindShootLineFromTo(parent.pawn.Position, new LocalTargetInfo(rangeEndPosition), out ShootLine _))
                    {
                        continue;
                    }

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

                List<IntVec3> targetedCells = GenSight.PointsOnLineOfSight(parent.pawn.Position, rangeEndPosition).Concat(endPosition).ToList(); //GetHitTiles(parent.pawn.Position, rangeEndPosition, parent.pawn.Map);
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
    }

    public class CompProperties_AbilityLaunchShotgunProjectiles : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityLaunchShotgunProjectiles()
        {
            compClass = typeof(CompAbilityEffect_LaunchShotgunProjectiles);
        }

        public override IEnumerable<string> ConfigErrors(AbilityDef parentDef)
        {
            if (downedHitChance != null)
            {
                Log.Warning(parentDef.defName + " is using downedHitChance which is deprecated and no longer in use. Please contact the mod's author and ask him to remove the said field.");
            }

            return base.ConfigErrors(parentDef);
        }

        // Projectile that you want to fire
        public ThingDef projectileDef;
        // Amount of pellets that your ability fires
        public int pelletCount;
        // Angle between fired pellets
        public float pelletAngle;
        // Randomly adjusts every pellet's fire angle up to this value
        public float pelletRandomSpread = 0f;
        // DEPRECATED
        public float? downedHitChance;
    }
}
