using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using static HarmonyLib.Code;
using UnityEngine;
using AthenaFramework.Gizmos;
using Verse.Sound;

namespace AthenaFramework
{
    public class HediffComp_Shield : HediffComp_Renderable
    {
        public float energy;
        public int ticksToReset = -1;
        public int lastImpactTick = -1;
        public int lastResetTick = -1;
        public bool freeRecharge = false; // Set to true in case you want your shield's next reboot to give it full energy
        public Vector3 impactAngleVect;

        private HediffCompProperties_Shield Props => props as HediffCompProperties_Shield;
        private static readonly float altitude = AltitudeLayer.MoteOverhead.AltitudeFor();

        public Matrix4x4 matrix;
        public Gizmo_HediffShieldStatus gizmo;

        public float EnergyPercent
        {
            get
            {
                return energy / Props.maxEnergy;
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset");
            Scribe_Values.Look(ref lastImpactTick, "lastImpactTick");
            Scribe_Values.Look(ref lastResetTick, "lastResetTick");
            Scribe_Values.Look(ref freeRecharge, "freeRecharge");
            Scribe_Values.Look(ref impactAngleVect, "impactAngleVect");
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            energy = Props.maxEnergy * Props.energyOnStart;
        }

        public virtual void BlockDamage(ref DamageInfo dinfo, ref bool absorbed)
        {
            if (ticksToReset > 0)
            {
                return;
            }

            if (Props.shatterOn.Contains(dinfo.Def))
            {
                Shatter(ref dinfo);
                return;
            }

            if ((dinfo.Def.isRanged && !Props.blocksRangedDamage) || (dinfo.Def.isExplosive && !Props.blocksExplosions) || (!dinfo.Def.isRanged && !dinfo.Def.isExplosive && !Props.blocksMeleeDamage))
            {
                return;
            }

            if (Props.blockDamageDefs != null)
            {
                if (!Props.blockDamageDefs.Contains(dinfo.Def))
                {
                    return;
                }
            }

            energy -= dinfo.Amount * Props.energyPerDamageModifier;

            if (energy <= 0)
            {
                Shatter(ref dinfo);

                if (Props.blockOverdamage)
                {
                    absorbed = true;
                    return;
                }

                if (Props.consumeOverdamage)
                {
                    dinfo.SetAmount(-1 * energy / Props.energyPerDamageModifier);
                }
                
                return;
            }

            absorbed = true;
            OnDamageAbsorb(ref dinfo);
        }

        public virtual void OnDamageAbsorb(ref DamageInfo dinfo)
        {
            Props.absorbSound.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 offsetVector = Pawn.DrawPos + this.impactAngleVect.RotatedBy(180f) * 0.5f;
            float damagePower = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(offsetVector, Pawn.MapHeld, Props.absorbFleck, damagePower);

            for (int i = 0; i < damagePower; i++)
            {
                FleckMaker.ThrowDustPuff(offsetVector, Pawn.MapHeld, Rand.Range(0.8f, 1.2f));
            }

            lastImpactTick = Find.TickManager.TicksGame;
        }

        public virtual bool BlockStun(ref DamageInfo dinfo)
        {
            if (ticksToReset > 0 || Props.absorbStuns == null)
            {
                return false;
            }

            if (!Props.absorbStuns.Contains(dinfo.Def))
            {
                return false;
            }

            energy -= dinfo.Amount * Props.energyPerStunModifier;

            if (energy <= 0)
            {
                Shatter(ref dinfo);

                if (Props.blockOverdamage)
                {
                    return true;
                }

                if (Props.consumeOverdamage)
                {
                    dinfo.SetAmount(-1 * energy / Props.energyPerStunModifier);
                }

                return false;
            }

            OnDamageAbsorb(ref dinfo);
            return true;
        }

        public virtual void Shatter(ref DamageInfo dinfo)
        {
            energy = 0;
            ticksToReset = Props.resetDelay;

            float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * EnergyPercent;
            if (Props.scaleWithOwner)
            {
                if (Pawn.RaceProps.Humanlike)
                {
                    scale *= Pawn.DrawSize.x;
                }
                else
                {
                    scale = (scale - 1) + Pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x;
                }
            }

            if (Props.shieldBreakEffecter != null)
            {
                Props.shieldBreakEffecter.SpawnAttached(Pawn, Pawn.MapHeld, scale * 0.5f);
            }

            if (Props.breakFleck != null)
            {
                FleckMaker.Static(Pawn.DrawPos, Pawn.Map, Props.breakFleck, 12f);
            }

            for (int i = 0; i < 6; i++)
            {
                FleckMaker.ThrowDustPuff(Pawn.DrawPos + Vector3Utility.HorizontalVectorFromAngle(Rand.Range(0, 360)) * Rand.Range(0.3f, 0.6f), Pawn.MapHeld, Rand.Range(0.8f, 1.2f));
            }

            if (Props.explosionOnShieldBreak)
            {
                GenExplosion.DoExplosion(Pawn.Position, Pawn.MapHeld, Props.explosionRadius, Props.explosionDef, Pawn);
            }
        }

        public virtual void Reset()
        {
            if (Pawn.Spawned)
            {
                Props.resetSound.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.MapHeld, false));
                FleckMaker.ThrowLightningGlow(Pawn.DrawPos, Pawn.MapHeld, 3f);
            }

            ticksToReset = -1;
            if (freeRecharge)
            {
                energy = Props.maxEnergy;
                freeRecharge = false;
            }
            else
            {
                energy = Props.maxEnergy * Props.energyOnReset;
            }

            lastResetTick = Find.TickManager.TicksGame;
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticksToReset > 0)
            {
                ticksToReset--;

                if (ticksToReset <= 0)
                {
                    this.Reset();
                }

                return;
            }

            if (energy >= Props.maxEnergy)
            {
                return;
            }

            energy = Math.Min(energy + Props.energyRechargeRate, Props.maxEnergy);
        }

        public override void DrawAt(Vector3 drawPos)
        {
            if (Props.graphicData == null)
            {
                return;
            }

            if (ticksToReset > 0)
            {
                return;
            }

            drawPos.y = altitude;
            float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * EnergyPercent;

            if (Props.scaleWithOwner)
            {
                if (Pawn.RaceProps.Humanlike)
                {
                    scale *= Pawn.DrawSize.x;
                }
                else
                {
                    scale = (scale - 1) + Pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize.x;
                }
            }

            if (lastImpactTick > Find.TickManager.TicksGame - 8)
            {
                float tickScaleModifier = (8 - Find.TickManager.TicksGame + lastImpactTick) / 8f * 0.05f;
                drawPos += impactAngleVect * tickScaleModifier;
                scale -= tickScaleModifier;

                if (lastResetTick > Find.TickManager.TicksGame - 20)
                {
                    tickScaleModifier = 1 - (20 - Find.TickManager.TicksGame + lastResetTick) / 20f;
                    scale *= tickScaleModifier;
                }
            }

            matrix.SetTRS(drawPos, Quaternion.AngleAxis(Props.spinning ? Rand.Range(0, 360) : 0, Vector3.up), new Vector3(scale, 1f, scale));
            Graphics.DrawMesh(MeshPool.plane10, matrix, Props.graphicData.Graphic.MatSingle, 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (Props.displayGizmo)
            {
                if (Pawn.Faction.IsPlayer && Find.Selector.SingleSelectedThing == Pawn)
                {
                    if (gizmo == null)
                    {
                        gizmo = new Gizmo_HediffShieldStatus();
                        gizmo.shieldHediff = this;
                    }

                    yield return gizmo;
                }
            }

            yield break;
        }
    }

    public class HediffCompProperties_Shield : HediffCompProperties_Renderable
    {
        public HediffCompProperties_Shield()
        {
            this.compClass = typeof(HediffComp_Shield);
        }

        // Maximum amount of energy that the shield can hold
        public float maxEnergy = 0f;
        // How much energy is recharged every tick
        public float energyRechargeRate = 0f;
        // How much energy is lost per unit of damage
        public float energyPerDamageModifier = 0.33f;
        // How long(in ticks) it takes for a shield to go back online after it has been destroyed
        public int resetDelay = 1;
        // What fraction of shield's max energy it has after resetting
        public float energyOnReset = 0.2f;
        // What fraction of shield's max energy should it start after being applied
        public float energyOnStart = 1f;
        // Whenever the shield blocks all damage/stun from the attack that breaks it or not
        public bool blockOverdamage = true;
        // Whenever the shield reduces damage/stun of the attack that broke it by what energy it had left(considering energyPerDamageModifier and energyPerStunModifier)
        public bool consumeOverdamage = false;

        // Whenever the shield blocks ranged/explosive/melee damage
        public bool blocksRangedDamage = true;
        public bool blocksExplosions = true;
        public bool blocksMeleeDamage = false;

        // If the shield should only block particular damage defs or not
        public List<DamageDef> blockDamageDefs;
        // What types of stuns the shield should block. If set to false no stuns will be blocked
        public List<DamageDef> absorbStuns;
        // How much energy is lost per unit of stun.
        public float energyPerStunModifier = 0.5f;

        // What types of damage should cause the shield to instantly shatter
        public List<DamageDef> shatterOn;
        // If the shield should create an explosion upon being destroyed
        public bool explosionOnShieldBreak = false;
        // Damage type and radius of the explosion
        public DamageDef explosionDef;
        public float explosionRadius = 2.9f;

        // Shield sounds and flecks
        public SoundDef absorbSound;
        public SoundDef resetSound;

        public FleckDef absorbFleck;
        public FleckDef breakFleck;
        // Effecter that's used upon shield shattering
        public EffecterDef shieldBreakEffecter;

        // Whenever the shield should display a charge gizmo and what text and hover tip should it have
        public bool displayGizmo = true;
        public string gizmoTitle = "";
        public string gizmoTip = "";

        // Shield scale based on amount of energy left
        public float minDrawSize = 1.2f;
        public float maxDrawSize = 1.55f;
        // Whenever the shield should scale with owner's draw size
        public bool scaleWithOwner = true;
        // Whenever the shield should have the vanilla spinning effect. Turn off in case you're using custom asymmetric textures
        public bool spinning = true;
    }
}
