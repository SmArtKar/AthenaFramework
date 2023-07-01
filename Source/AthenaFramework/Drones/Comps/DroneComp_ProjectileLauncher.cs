using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class DroneComp_ProjectileLauncher : DroneComp
    {
        protected DroneCompProperties_ProjectileLauncher Props => props as DroneCompProperties_ProjectileLauncher;

        public int warmupTicksLeft = 0;
        public int burstShotsLeft = 0;
        public int burstTicksLeft = 0;
        public bool bursting = false;

        public Vector3 recoil;

        public LocalTargetInfo target;

        public Sustainer aimingSustainer;
        public Effecter aimingEffecter;

        private CompReloadable cachedReloadable;
        private CompChangeableProjectile cachedChangeableProjectile;

        public virtual ThingDef Projectile
        {
            get
            {
                if (Props.useChangeableProjectile && ParentChangeableProjectile != null && ParentChangeableProjectile.Loaded)
                {
                    return ParentChangeableProjectile.Projectile;
                }

                return Props.projectileDef;
            }
        }

        public virtual int WarmupTicks
        {
            get
            {
                return (int)Math.Floor(Props.aimingTime * 60);
            }
        }

        public virtual int BurstShotsCount
        {
            get
            {
                return Props.burstShotCount;
            }
        }

        public virtual float MaxRange
        {
            get
            {
                return Props.range * parent.AttackRangeMultiplier;
            }
        }

        public virtual float MinRange
        {
            get
            {
                return Props.minRange * parent.AttackRangeMultiplier;
            }
        }

        public virtual float ForcedMissRadius
        {
            get
            {
                return Props.forcedMissRadius;
            }
        }

        public CompReloadable ParentReloadable
        {
            get
            {
                if (parent.EquipmentSource == null)
                {
                    return null;
                }

                if (cachedReloadable == null)
                {
                    cachedReloadable = parent.EquipmentSource.GetComp<CompReloadable>();
                }

                return cachedReloadable;
            }
        }

        public CompChangeableProjectile ParentChangeableProjectile
        {
            get
            {
                if (parent.EquipmentSource != null)
                {
                    return null;
                }

                if (cachedChangeableProjectile == null)
                {
                    cachedChangeableProjectile = parent.EquipmentSource.GetComp<CompChangeableProjectile>();
                }

                return cachedChangeableProjectile;
            }
        }

        public override void TargetUpdate()
        {
            base.TargetUpdate();
            target = parent.CurrentTarget;
        }

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (recoil != Vector3.zero)
            {
                recoil = recoil.normalized * Math.Max(0, recoil.magnitude - Props.recoilRecovery);
            }

            if (Props.useReloadable && ParentReloadable != null && !ParentReloadable.CanBeUsed)
            {
                return;
            }

            if (target == null)
            {
                target = parent.CurrentTarget;
            }

            if (!IsValidTarget(target))
            {
                InvalidTarget();
                return;
            }

            if (bursting)
            {
                BurstingTick();
                return;
            }

            if (warmupTicksLeft == 0)
            {
                StartWarmup();
            }

            WarmupTick();
        }

        public virtual void WarmupTick()
        {
            warmupTicksLeft -= 1;

            if (warmupTicksLeft <= 0)
            {
                StartBurst();
                return;
            }

            if (aimingSustainer != null)
            {
                aimingSustainer.Maintain();
            }

            if (aimingEffecter != null)
            {
                aimingEffecter.EffectTick(Pawn, Pawn);
            }

        }

        public override Vector3 DrawPosOffset()
        {
            return base.DrawPosOffset() + recoil;
        }

        public virtual void StartBurst()
        {
            if (!IsValidTarget(target))
            {
                InvalidTarget();
                return;
            }

            if (aimingEffecter != null)
            {
                aimingSustainer.End();
            }

            if (aimingEffecter != null)
            {
                aimingEffecter.Cleanup();
            }

            burstShotsLeft = BurstShotsCount;
            burstTicksLeft = 0;
            bursting = true;
        }

        public virtual void BurstingTick()
        {
            burstTicksLeft -= 1;

            if (burstTicksLeft > 0)
            {
                return;
            }

            if (!Shoot(target))
            {
                InvalidTarget();
                return;
            }

            burstShotsLeft -= 1;

            if (burstShotsLeft <= 0)
            {
                StartWarmup();
            }
        }

        public virtual void StartWarmup()
        {
            bursting = false;
            warmupTicksLeft = WarmupTicks;

            if (Props.soundAiming != null)
            {
                aimingSustainer = Props.soundAiming.TrySpawnSustainer(Pawn);
            }

            if (Props.aimingEffecter != null)
            {
                aimingEffecter = Props.aimingEffecter.SpawnAttached(Pawn, Pawn.Map);
            }
        }

        public virtual void InvalidTarget()
        {
            if (aimingEffecter != null)
            {
                aimingSustainer.End();
            }

            if (aimingEffecter != null)
            {
                aimingEffecter.Cleanup();
            }

            burstShotsLeft = 0;
            burstTicksLeft = 0;
            bursting = false;
            target = null;
        }

        public virtual IntVec3 GetForcedMissTarget(float forcedMissRadius)
        {
            int max = GenRadial.NumCellsInRadius(forcedMissRadius);
            return target.Cell + GenRadial.RadialPattern[Rand.Range(0, max)];
        }

        public virtual bool IsValidTarget(LocalTargetInfo target)
        {
            if (target == null || !target.IsValid)
            {
                return false;
            }

            IntVec3 position = parent.CurrentPosition;

            if (target.HasThing && target.Thing.Map != Pawn.Map)
            {
                return false;
            }

            if (target.Cell.DistanceToSquared(position) > MaxRange * MaxRange)
            {
                return false;
            }

            if (target.Cell.DistanceToSquared(position) < MinRange * MinRange)
            {
                return false;
            }

            if (bursting)
            {
                if (Props.stopBurstWithoutLos && !GenSight.LineOfSight(position, target.Cell, Pawn.Map, true))
                {
                    return false;
                }
            }
            else if (Props.requireLineOfSight && !GenSight.LineOfSight(position, target.Cell, Pawn.Map, true))
            {
                return false;
            }

            return true;
        }

        public override void OnRecalled()
        {
            base.OnRecalled();
            bursting = false;
            burstShotsLeft = 0;
            burstTicksLeft = 0;
            warmupTicksLeft = 0;
            target = null;
            recoil = Vector3.zero;
        }

        public virtual bool Shoot(LocalTargetInfo target)
        {
            if (!IsValidTarget(target))
            {
                return false;
            }

            Find.BattleLog.Add(new BattleLogEntry_RangedFire(Pawn, target.HasThing ? target.Thing : null, parent.EquipmentSource?.def, Projectile, BurstShotsCount > 1));

            Props.soundShot.PlayOneShot(Pawn);
            Props.shotEffecter.Spawn(Pawn, Pawn.Map).Cleanup();

            ThingDef projectileDef = Projectile;

            if (projectileDef == null)
            {
                return false;
            }

            IntVec3 position = parent.CurrentPosition;

            bool flag = AthenaCombatUtility.GetShootLine(position, Pawn.Map, target, out ShootLine resultingLine);

            if (Props.stopBurstWithoutLos && !flag)
            {
                return false;
            }

            recoil += (Pawn.Position - target.Cell).ToVector3().normalized * Props.recoil;

            Vector3 drawPos = parent.DrawPos;
            Projectile projectile = (Projectile)GenSpawn.Spawn(projectileDef, resultingLine.Source, Pawn.Map);

            if (ForcedMissRadius > 0.5f)
            {
                float forcedMiss = VerbUtility.CalculateAdjustedForcedMiss(ForcedMissRadius, target.Cell - position);

                if (forcedMiss > 0.5f)
                {
                    IntVec3 forcedMissTarget = GetForcedMissTarget(forcedMiss);

                    if (forcedMissTarget != target.Cell)
                    {
                        ProjectileHitFlags projectileHitFlags = ProjectileHitFlags.NonTargetWorld;

                        if (Rand.Chance(0.5f))
                        {
                            projectileHitFlags = ProjectileHitFlags.All;
                        }

                        projectile.Launch(Pawn, drawPos, forcedMissTarget, target, projectileHitFlags, Props.preventFriendlyFire, parent.EquipmentSource);

                        if (Props.useReloadable && ParentReloadable != null)
                        {
                            ParentReloadable.UsedOnce();
                        }

                        if (Props.useChangeableProjectile && ParentChangeableProjectile != null)
                        {
                            ParentChangeableProjectile.Notify_ProjectileLaunched();
                        }

                        return true;
                    }
                }
            }

            float hitChance = parent.GetRangedHitChance(target.Cell.DistanceTo(position));

            if (target.HasThing)
            {
                HitChanceFlags hitFlags = HitChanceFlags.Posture | HitChanceFlags.Gas | HitChanceFlags.Weather | HitChanceFlags.Size | HitChanceFlags.Execution;

                if (Props.useOwnerRangedSkill)
                {
                    hitFlags |= HitChanceFlags.ShooterStats;
                }

                hitChance *= AthenaCombatUtility.GetRangedHitChance(position, target.Thing, hitFlags, Pawn);
            }

            Thing randomCover = AthenaCombatUtility.GetRandomCoverToMissInto(target, position, Pawn.Map);

            if (Props.canGoWild && !Rand.Chance(hitChance))
            {
                resultingLine.ChangeDestToMissWild(hitChance);
                ProjectileHitFlags projectileHitFlags2 = ProjectileHitFlags.NonTargetWorld;
                if (Rand.Chance(0.5f))
                {
                    projectileHitFlags2 |= ProjectileHitFlags.NonTargetPawns;
                }
                projectile.Launch(Pawn, drawPos, resultingLine.Dest, target, projectileHitFlags2, Props.preventFriendlyFire, parent.EquipmentSource, randomCover.def);

                if (Props.useReloadable && ParentReloadable != null)
                {
                    ParentReloadable.UsedOnce();
                }

                if (Props.useChangeableProjectile && ParentChangeableProjectile != null)
                {
                    ParentChangeableProjectile.Notify_ProjectileLaunched();
                }

                return true;
            }

            if (target.Thing != null && target.Thing.def.CanBenefitFromCover && !Rand.Chance(1 - CoverUtility.CalculateOverallBlockChance(target, position, Pawn.Map)))
            {
                ProjectileHitFlags projectileHitFlags3 = ProjectileHitFlags.NonTargetWorld | ProjectileHitFlags.NonTargetPawns;
                projectile.Launch(Pawn, drawPos, randomCover, target, projectileHitFlags3, Props.preventFriendlyFire, parent.EquipmentSource, randomCover.def);

                if (Props.useReloadable && ParentReloadable != null)
                {
                    ParentReloadable.UsedOnce();
                }

                if (Props.useChangeableProjectile && ParentChangeableProjectile != null)
                {
                    ParentChangeableProjectile.Notify_ProjectileLaunched();
                }

                return true;
            }

            ProjectileHitFlags projectileHitFlags4 = ProjectileHitFlags.IntendedTarget | ProjectileHitFlags.NonTargetPawns;

            if (!target.HasThing || target.Thing.def.Fillage == FillCategory.Full)
            {
                projectileHitFlags4 |= ProjectileHitFlags.NonTargetWorld;
            }

            if (target.Thing != null)
            {
                projectile.Launch(Pawn, drawPos, target, target, projectileHitFlags4, Props.preventFriendlyFire, parent.EquipmentSource, randomCover.def);

                if (Props.useReloadable && ParentReloadable != null)
                {
                    ParentReloadable.UsedOnce();
                }

                if (Props.useChangeableProjectile && ParentChangeableProjectile != null)
                {
                    ParentChangeableProjectile.Notify_ProjectileLaunched();
                }

                return true;
            }

            projectile.Launch(Pawn, drawPos, resultingLine.Dest, target, projectileHitFlags4, Props.preventFriendlyFire, parent.EquipmentSource, randomCover.def);

            if (Props.useReloadable && ParentReloadable != null)
            {
                ParentReloadable.UsedOnce();
            }

            if (Props.useChangeableProjectile && ParentChangeableProjectile != null)
            {
                ParentChangeableProjectile.Notify_ProjectileLaunched();
            }

            return true;
        }
    }

    public class DroneCompProperties_ProjectileLauncher : DroneCompProperties
    {
        public float minRange;
        public float range = 1.42f;

        public int burstShotCount = 1;
        public int ticksBetweenBurstShots = 15;
        public bool requireLineOfSight = true;
        public bool stopBurstWithoutLos = true;

        // In seconds
        public float aimingTime = 1.5f;
        public float forcedMissRadius = 0f;
        public bool preventFriendlyFire = false;
        public bool canGoWild = true;

        // Whenever the drone should use CompChangeableProjectile or CompReloadable found on its equipment source
        public bool useReloadable = false;
        public bool useChangeableProjectile = true;

        // When set to true, drone will additionally use owner's ranged hit chance
        public bool useOwnerRangedSkill = false;

        public SoundDef soundAiming;
        public SoundDef soundShot;
        public EffecterDef aimingEffecter;
        public EffecterDef shotEffecter;

        // How much recoil is applied (in tiles) with each shot and how much of it is removed each tick
        // Recoil is purely visual and has no gameplay impact
        public float recoil = 0.1f;
        public float recoilRecovery = 0.01f;

        public ThingDef projectileDef;

        public DroneCompProperties_ProjectileLauncher()
        {
            compClass = typeof(DroneComp_ProjectileLauncher);
        }
    }
}
