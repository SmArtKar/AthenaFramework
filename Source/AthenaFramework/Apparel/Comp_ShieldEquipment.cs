using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class Comp_ShieldEquipment : Comp_AdditionalApparelGraphics
    {
        private CompProperties_ShieldEquipment Props => props as CompProperties_ShieldEquipment;
        private Apparel Apparel => parent as Apparel;
        private Pawn Pawn => Apparel.Wearer;

        public float energy;
        public int ticksToReset = -1;
        public int lastImpactTick = -1;
        public int lastResetTick = -1;
        public bool freeRecharge = false; // Set to true in case you want your shield's next reboot to give it full energy
        public Vector3 impactAngleVect;

        public Matrix4x4 matrix;
        public Gizmo_ShieldStatus gizmo;

        public float EnergyPercent
        {
            get
            {
                return energy / MaxEnergy;
            }
        }

        public virtual float MaxEnergy
        {
            get
            {
                return Props.maxEnergy * parent.GetStatValue(StatDefOf.EnergyShieldEnergyMax);
            }
        }

        public virtual float EnergyRechargeRate
        {
            get
            {
                return Props.energyRechargeRate + parent.GetStatValue(StatDefOf.EnergyShieldRechargeRate) / 60f;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref energy, "energy");
            Scribe_Values.Look(ref ticksToReset, "ticksToReset");
            Scribe_Values.Look(ref lastImpactTick, "lastImpactTick");
            Scribe_Values.Look(ref lastResetTick, "lastResetTick");
            Scribe_Values.Look(ref freeRecharge, "freeRecharge");
            Scribe_Values.Look(ref impactAngleVect, "impactAngleVect");
        }


        public virtual void OnDamageAbsorb(ref DamageInfo dinfo)
        {
            Props.absorbSound.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map, false));
            impactAngleVect = Vector3Utility.HorizontalVectorFromAngle(dinfo.Angle);
            Vector3 offsetVector = Pawn.DrawPos + impactAngleVect.RotatedBy(180f) * 0.5f;
            float damagePower = Mathf.Min(10f, 2f + dinfo.Amount / 10f);
            FleckMaker.Static(offsetVector, Pawn.MapHeld, Props.absorbFleck, damagePower);

            for (int i = 0; i < damagePower; i++)
            {
                FleckMaker.ThrowDustPuff(offsetVector, Pawn.MapHeld, Rand.Range(0.8f, 1.2f));
            }

            lastImpactTick = Find.TickManager.TicksGame;
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
                energy = MaxEnergy;
                freeRecharge = false;
            }
            else
            {
                energy = MaxEnergy * Props.energyOnReset;
            }

            lastResetTick = Find.TickManager.TicksGame;
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            energy = MaxEnergy;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (ticksToReset > 0)
            {
                ticksToReset--;

                if (ticksToReset <= 0)
                {
                    Reset();
                }

                return;
            }

            if (energy >= MaxEnergy)
            {
                return;
            }

            energy = Math.Min(energy + EnergyRechargeRate, MaxEnergy);
        }

        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            base.PostPreApplyDamage(dinfo, out absorbed);

            if (ticksToReset > 0)
            {
                return;
            }

            if (Props.shatterOn != null && Props.shatterOn.Contains(dinfo.Def))
            {
                Shatter(ref dinfo);
                return;
            }

            bool overrideTypechecks = false;

            if (Props.damageInfoPacks != null)
            {
                for (int i = Props.damageInfoPacks.Count - 1; i >= 0; i--)
                {
                    DamageInfoPack pack = Props.damageInfoPacks[i];

                    if (pack.damageDef != dinfo.Def)
                    {
                        continue;
                    }

                    overrideTypechecks = true;

                    if ((dinfo.Def.isRanged && !pack.blocksRangedDamage) || (dinfo.Def.isExplosive && !pack.blocksExplosions) || (!dinfo.Def.isRanged && !dinfo.Def.isExplosive && !pack.blocksMeleeDamage))
                    {
                        return;
                    }
                }
            }

            if (!overrideTypechecks)
            {
                if ((dinfo.Def.isRanged && !Props.blocksRangedDamage) || (dinfo.Def.isExplosive && !Props.blocksExplosions) || (!dinfo.Def.isRanged && !dinfo.Def.isExplosive && !Props.blocksMeleeDamage))
                {
                    return;
                }

                if (Props.whitelistedDamageDefs != null && !Props.whitelistedDamageDefs.Contains(dinfo.Def))
                {
                    return;
                }

                if (Props.blacklistedDamageDefs != null && Props.blacklistedDamageDefs.Contains(dinfo.Def))
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

        public override void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
            if (Props.onlyRenderWhenDrafted && (Pawn.drafter == null || !Pawn.drafter.Drafted))
            {
                return;
            }

            if (Props.graphicData == null)
            {
                DrawSecondaries(drawPos, bodyType);
                return;
            }

            if (ticksToReset > 0)
            {
                return;
            }

            drawPos.y = Props.altitude.AltitudeFor();
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

            DrawSecondaries(drawPos, bodyType);
        }

        public override void DrawSecondaries(Vector3 drawPos, BodyTypeDef bodyType)
        {
            if (Props.additionalGraphics == null)
            {
                return;
            }

            if (additionalGraphics == null)
            {
                RecacheGraphicData();
            }

            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                ApparelGraphicPackage package = additionalGraphics[i];

                if (!package.CanRender(Apparel, bodyType, Pawn))
                {
                    continue;
                }

                Vector3 offset = new Vector3();

                if (package.offsets != null)
                {
                    if (package.offsets.Count == 4)
                    {
                        offset = package.offsets[Pawn.Rotation.AsInt];
                    }
                    else
                    {
                        offset = package.offsets[0];
                    }
                }

                package.GetGraphic(Apparel, bodyType, null, EnergyPercent).Draw(drawPos + offset, Pawn.Rotation, Pawn);
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }

            if (Props.displayGizmo)
            {
                if (Pawn.Faction.IsPlayer && Find.Selector.SingleSelectedThing == Pawn)
                {
                    if (gizmo == null)
                    {
                        gizmo = new Gizmo_ShieldStatus(this);
                    }

                    yield return gizmo;
                }
            }

            yield break;
        }
    }

    public class CompProperties_ShieldEquipment : CompProperties_AdditionalApparelGraphics
    {
        public CompProperties_ShieldEquipment()
        {
            this.compClass = typeof(Comp_ShieldEquipment);
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
        // Whenever the shield blocks all damage/stun from the attack that breaks it or not
        public bool blockOverdamage = true;
        // Whenever the shield reduces damage/stun of the attack that broke it by what energy it had left(considering energyPerDamageModifier and energyPerStunModifier)
        public bool consumeOverdamage = false;

        // Whenever the shield blocks ranged/explosive/melee damage
        public bool blocksRangedDamage = true;
        public bool blocksExplosions = true;
        public bool blocksMeleeDamage = false;

        // List of whitelisted DamageDefs. When set, DamageDefs that are not in this list won't be affected.
        public List<DamageDef> whitelistedDamageDefs;
        // List of blacklisted DamageDefs. When set, DamageDefs that are in this list won't be affected.
        public List<DamageDef> blacklistedDamageDefs;

        // List of DamageInfoPack with additional energy cosumption modifiers and overrides for block types for certain DamageDefs
        public List<DamageInfoPack> damageInfoPacks;

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
