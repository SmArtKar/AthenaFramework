using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class DroneComp_ProjectileAttack : DroneComp
    {
        protected DroneCompProperties_ProjectileAttack Props => props as DroneCompProperties_ProjectileAttack;

        public int warmupTicksLeft = 0;
        public int burstShotsLeft = 0;
        public int burstTicksLeft = 0;
        public bool bursting = false;

        public LocalTargetInfo target;

        public Sustainer aimingSustainer;
        public Effecter aimingEffecter;

        public virtual ThingDef Projectile
        {
            get
            {
                if (parent.EquipmentSource != null)
                {
                    CompChangeableProjectile comp = parent.EquipmentSource.GetComp<CompChangeableProjectile>();

                    if (comp != null && comp.Loaded)
                    {
                        return comp.Projectile;
                    }
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

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (target == null)
            {
                target = parent.CurrentTarget;

                if (target != null && !IsValidTarget(target))
                {
                    target = null;
                    return;
                }

                StartWarmup();
            }

            if (!bursting)
            {
                WarmupTick();
                return;
            }

            BurstingTick();
        }

        public virtual void WarmupTick()
        {
            warmupTicksLeft -= 1;

            if (warmupTicksLeft > 0)
            {
                if (aimingSustainer != null)
                {
                    aimingSustainer.Maintain();
                }

                if (aimingEffecter != null)
                {
                    aimingEffecter.EffectTick(Pawn, Pawn);
                }

                return;
            }

            StartBurst();
        }

        public virtual void StartBurst()
        {
            if (!IsValidTarget(target))
            {
                ClearTarget();
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

            if (!IsValidTarget(target))
            {
                ClearTarget();
                return;
            }

            Shoot(target);
            burstShotsLeft -= 1;

            if (burstShotsLeft == 0)
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
                aimingEffecter = Props.aimingEffecter.SpawnAttached(Pawn, Pawn.MapHeld);
            }
        }

        public virtual void ClearTarget()
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
            if (target.Cell.DistanceToSquared(Pawn.PositionHeld) > MaxRange * MaxRange)
            {
                return false;
            }

            if (target.Cell.DistanceToSquared(Pawn.PositionHeld) < MinRange * MinRange)
            {
                return false;
            }

            if (bursting)
            {
                if (Props.stopBurstWithoutLos && !GenSight.LineOfSight(Pawn.PositionHeld, target.Cell, Pawn.MapHeld, true))
                {
                    return false;
                }
            }
            else if (Props.requireLineOfSight && !GenSight.LineOfSight(Pawn.PositionHeld, target.Cell, Pawn.MapHeld, true))
            {
                return false;
            }

            return true;
        }

        public virtual void Shoot(LocalTargetInfo target)
        {
            Find.BattleLog.Add(new BattleLogEntry_RangedFire(Pawn, target.HasThing ? target.Thing : null, (parent.EquipmentSource != null) ? parent.EquipmentSource.def : null, Projectile, BurstShotsCount > 1));

            Props.soundShot.PlayOneShot(Pawn);
            Props.shotEffecter.Spawn(Pawn, Pawn.MapHeld).Cleanup();

            ThingDef projectile = Projectile;

            if (projectile == null)
            {
                return;
            }
        }
    }

    public class DroneCompProperties_ProjectileAttack : DroneCompProperties
    {
        public float minRange;
        public float range = 1.42f;

        public int burstShotCount = 1;
        public int ticksBetweenBurstShots = 15;
        public bool requireLineOfSight = true;
        public bool stopBurstWithoutLos = true;
        // In seconds
        public float aimingTime = 1.5f;

        public SoundDef soundAiming;
        public SoundDef soundShot;
        public EffecterDef aimingEffecter;
        public EffecterDef shotEffecter;

        public ThingDef projectileDef;

        public DroneCompProperties_ProjectileAttack()
        {
            compClass = typeof(DroneComp_ProjectileAttack);
        }
    }
}
