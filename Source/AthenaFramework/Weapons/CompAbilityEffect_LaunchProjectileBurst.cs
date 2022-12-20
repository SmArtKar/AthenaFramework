using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompAbilityEffect_LaunchProjectileBurst : CompAbilityEffect
    {
        private CompProperties_AbilityLaunchProjectileBurst NewProps => props as CompProperties_AbilityLaunchProjectileBurst;

        public int ticksToShot = -1;
        public int shotsLeft = 0;
        public LocalTargetInfo curTarget;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            shotsLeft = NewProps.burstCount;
            curTarget = target;
        }

        public override void CompTick()
        {
            base.CompTick();
            if (shotsLeft == 0)
            {
                return;
            }

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
            ticksToShot = NewProps.ticksBetweenShots;
            LaunchProjectile(curTarget);

            if (shotsLeft == 0)
            {
                curTarget = null;
            }
        }

        public virtual void LaunchProjectile(LocalTargetInfo target)
        {
            Projectile proj = GenSpawn.Spawn(NewProps.projectileDef, parent.pawn.Position, parent.pawn.Map) as Projectile;
            proj.Launch(parent.pawn, parent.pawn.DrawPos, target, target, ProjectileHitFlags.IntendedTarget);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            return target.Pawn != null;
        }

        public CompAbilityEffect_LaunchProjectileBurst() { }
    }

    public class CompProperties_AbilityLaunchProjectileBurst : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityLaunchProjectileBurst()
        {
            this.compClass = typeof(CompAbilityEffect_LaunchProjectileBurst);
        }

        public ThingDef projectileDef;
        public int burstCount = 1;
        public int ticksBetweenShots = 5;
    }
}
