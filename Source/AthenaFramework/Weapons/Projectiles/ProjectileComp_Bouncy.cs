using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class ProjectileComp_Bouncy : ProjectileComp
    {
        private CompProperties_ProjectileBouncy Props => props as CompProperties_ProjectileBouncy;

        public int bouncesLeft = 0;
        public IntVec3 lastPosition;

        public Vector3 TargetDir
        {
            get
            {
                Vector3 targetVector = (Projectile.destination - Projectile.origin).normalized;
                Vector3 normalVector = (Projectile.destination - lastPosition.ToVector3()).normalized;
                return Vector3.Reflect(targetVector, normalVector);
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            bouncesLeft = Props.bounceAmount;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Projectile.Position != Projectile.DestinationCell)
            {
                lastPosition = Projectile.Position;
            }
        }

        public override void Impact(Thing hitThing, ref bool blockedByShield)
        {
            base.Impact(hitThing, ref blockedByShield);

            if (bouncesLeft == 0 || hitThing == null)
            {
                return;
            }

            Thing target = null;

            IntVec3 targetPosition = lastPosition + (TargetDir * Props.range).ToIntVec3();
            List<IntVec3> lineOfSight = GenSight.PointsOnLineOfSight(lastPosition, targetPosition).ToList();
            for (int i = 0; i < lineOfSight.Count; i++)
            {
                IntVec3 tile = lineOfSight[i];

                if (!tile.IsValid)
                {
                    break;
                }

                Thing targetBuilding = tile.GetRoofHolderOrImpassable(Projectile.Map);
                if (targetBuilding != null)
                {
                    target = targetBuilding;
                    break;
                }

                Thing cover = tile.GetCover(Projectile.Map);
                if (cover != null)
                {
                    if (Rand.Chance(cover.BaseBlockChance()))
                    {
                        target = targetBuilding;
                        break;
                    }
                }

                List<Thing> thingList = GridsUtility.GetThingList(tile, Projectile.Map);
                for (int k = thingList.Count - 1; k >= 0; k--)
                {
                    Pawn pawnTarget = thingList[k] as Pawn;

                    if (pawnTarget == null)
                    {
                        continue;
                    }

                    HitChanceFlags flags = HitChanceFlags.Size | HitChanceFlags.Posture | HitChanceFlags.Execution;

                    if (Rand.Chance(AthenaArmor.GetHitChance(lastPosition, pawnTarget, flags)))
                    {
                        target = pawnTarget;
                        break;
                    }
                }

                if (target != null)
                {
                    break;
                }
            }

            if (target == null)
            {
                return;
            }

            Projectile newProj = GenSpawn.Spawn(Projectile.def, lastPosition, Projectile.Map) as Projectile;
            ProjectileComp_Bouncy comp = newProj.GetComp<ProjectileComp_Bouncy>();
            comp.bouncesLeft = bouncesLeft - 1;
            newProj.Launch(Projectile, target != null ? target : lineOfSight.Last(), target != null ? target : lineOfSight.Last(), ProjectileHitFlags.All);
        }
    }

    public class CompProperties_ProjectileBouncy : CompProperties
    {
        public CompProperties_ProjectileBouncy()
        {
            this.compClass = typeof(ProjectileComp_Bouncy);
        }

        public int bounceAmount = 3;
        public float range = 15.9f;
    }
}
