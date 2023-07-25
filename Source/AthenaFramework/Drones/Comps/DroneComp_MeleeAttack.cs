using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.GridBrushBase;

namespace AthenaFramework
{
    public class DroneComp_MeleeAttack : DroneComp
    {
        private DroneCompProperties_MeleeAttack Props => props as DroneCompProperties_MeleeAttack;

        public Vector3 attackAnimation;
        public int attackCooldown;

        public virtual float MeleeDamageFor(LocalTargetInfo target)
        {
            return Props.meleeDamage * Props.damageRandomFactor.RandomInRange;
        }

        public virtual float ArmorPenetrationFor(LocalTargetInfo target)
        {
            return Props.armorPenetration;
        }

        public virtual int CooldownFor(LocalTargetInfo target)
        {
            return (int)(Props.cooldown * 60);
        }

        public virtual DamageDef DamageDefFor(LocalTargetInfo target)
        {
            return Props.damageDef;
        }

        public virtual float HitChanceFor(LocalTargetInfo target)
        {
            if (Props.usePawnMelee)
            {
                return AthenaCombatUtility.GetMeleeHitChance(Pawn, target);
            }

            if (AthenaCombatUtility.IsTargetImmobile(target))
            {
                return 1f;
            }

            return Props.hitChance;
        }

        public virtual IEnumerable<DamageInfo> DamageInfosToApply(LocalTargetInfo target)
        {
            float damage = MeleeDamageFor(target);
            float armorPen = ArmorPenetrationFor(target);
            DamageDef damageDef = DamageDefFor(target);

            if (damage < 1f)
            {
                damage = 1f;
                damageDef = DamageDefOf.Blunt;
            }

            Vector3 direction = (target.Thing.Position - parent.CurrentPosition).ToVector3();
            DamageInfo damageInfo = new DamageInfo(damageDef, damage, armorPen, -1f, Pawn, null, parent.EquipmentSource?.def, DamageInfo.SourceCategory.ThingOrUnknown, null, !Pawn.Drafted);
            damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            damageInfo.SetAngle(direction);
            yield return damageInfo;
        }

        public virtual DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            DamageWorker.DamageResult result = new DamageWorker.DamageResult();

            foreach (DamageInfo item in DamageInfosToApply(target))
            {
                if (!target.ThingDestroyed)
                {
                    result = target.Thing.TakeDamage(item);
                    continue;
                }

                return result;
            }

            return result;
        }

        public virtual SoundDef SoundDodge(Thing target)
        {
            if (target.def.race != null && target.def.race.soundMeleeDodge != null)
            {
                return target.def.race.soundMeleeDodge;
            }

            return Props.soundMiss;
        }

        public virtual bool CanAttack(LocalTargetInfo target)
        {
            if (!target.IsValid || target.Thing == null || !target.Thing.Spawned)
            {
                return false;
            }

            if (target.Cell.DistanceToSquared(parent.CurrentPosition) > 2.1f)
            {
                return false;
            }

            return true;
        }

        public virtual void Attack(Thing target)
        {
            Pawn pawn = target as Pawn;

            attackCooldown = CooldownFor(target);

            if (pawn != null && !pawn.Dead)
            {
                pawn.stances.stagger.StaggerFor(95);
            }

            if (!Rand.Chance(HitChanceFor(target)))
            {
                if (Props.soundMiss != null)
                {
                    Props.soundMiss.PlayOneShot(new TargetInfo(target.Position, Pawn.Map));
                }

                return;
            }

            if (!Rand.Chance(AthenaCombatUtility.GetDodgeChance(target)))
            {
                MoteMaker.ThrowText(target.DrawPos, Pawn.Map, "TextMote_Dodge".Translate(), 1.9f);
                if (target.def.race != null && target.def.race.soundMeleeDodge != null)
                {
                    target.def.race.soundMeleeDodge.PlayOneShot(new TargetInfo(target.Position, Pawn.Map));
                    return;
                }

                if (Props.soundMiss != null)
                {
                    Props.soundMiss.PlayOneShot(new TargetInfo(target.Position, Pawn.Map));
                }

                return;
            }

            if (Props.impactMote != null)
            {
                MoteMaker.MakeStaticMote(target.DrawPos, Pawn.Map, Props.impactMote);
            }

            if (Props.impactFleck != null)
            {
                FleckMaker.Static(target.DrawPos, Pawn.Map, Props.impactFleck);
            }

            DamageWorker.DamageResult damageResult = ApplyMeleeDamageToTarget(target);

            if (pawn != null && damageResult.totalDamageDealt > 0f)
            {
                AthenaCombatUtility.ApplyMeleeSlaveSuppression(Pawn, pawn, damageResult.totalDamageDealt);
            }
        }

        public override void OnDeployed()
        {
            base.OnDeployed();
            attackCooldown = (int)(Props.cooldown * 60);
            attackAnimation = Vector3.zero;
        }

        public override void PrePawnApplyDamage(ref DamageInfo dinfo, ref float hitChance, ref bool absorbed)
        {
            base.PrePawnApplyDamage(ref dinfo, ref hitChance, ref absorbed);

            if ((dinfo.Def.isRanged && !Props.parryRanged) || dinfo.Def.isExplosive)
            {
                return;
            }

            if (parent.CurrentPosition != Pawn.Position) //Cannot block if we're mid ranged attack
            {
                return;
            }

            if (Rand.Chance(Props.riposteChance) && dinfo.Instigator != null && !dinfo.Def.isRanged)
            {
                if (CanAttack(dinfo.Instigator))
                {
                    Attack(dinfo.Instigator);

                    if (Props.blockOnRiposte)
                    {
                        absorbed = true;
                        return;
                    }
                }
            }

            if (Rand.Chance(Props.meleeBlockChance))
            {
                absorbed = true;

                if (Props.impactMote != null)
                {
                    MoteMaker.MakeStaticMote(Pawn.DrawPos, Pawn.Map, Props.impactMote);
                }

                if (Props.impactFleck != null)
                {
                    FleckMaker.Static(Pawn.DrawPos, Pawn.Map, Props.impactFleck);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref attackCooldown, "attackCooldown");
        }

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (attackCooldown > 0)
            {
                attackCooldown -= 1;
                return;
            }

            if (CanAttack(parent.CurrentTarget))
            {
                Attack(parent.CurrentTarget.Thing);
            }
        }
    }

    public class DroneCompProperties_MeleeAttack : DroneCompProperties
    {
        public float meleeDamage;
        public float armorPenetration;
        public DamageDef damageDef;

        public SoundDef soundHitPawn;
        public SoundDef soundHitBuilding;
        public SoundDef soundMiss;

        public ThingDef impactMote;
        public FleckDef impactFleck;

        // Equal to level 15 melee hit chance
        public float hitChance = 0.85f;
        // Attack cooldown, in seconds. If set to -1, then the drone won't attack by itself
        public float cooldown = 1.2f;
        // If set to true, then the drone will use owner's MeleeHitChance instead of hitChance
        public bool usePawnMelee = false;

        // Attack animation power, in tiles
        public float animationPower = 0.1f;
        // How quickly the drone recovers from the attack animation, tiles per tick
        public float animationRecovery = 0.01f;

        // How likely the drone is to retaliate upon any melee attack
        public float riposteChance = 0f;
        // How likely the drone is to block an incoming melee attack
        public float meleeBlockChance = 0f;
        public bool blockOnRiposte = true;

        // If the drone can parry projectiles
        public bool parryRanged = false;

        // Random factor for melee damage, 0.8-1.2 is vanilla values
        public FloatRange damageRandomFactor = new FloatRange(0.8f, 1.2f);

        public DroneCompProperties_MeleeAttack()
        {
            compClass = typeof(DroneComp_MeleeAttack);
        }
    }
}
