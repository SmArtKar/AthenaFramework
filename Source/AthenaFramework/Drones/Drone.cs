using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Runtime.InteropServices;
using UnityEngine;
using Mono.Unix.Native;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class Drone : IRenderable, IColorSelector, IDamageResponse, IExposable
    {
        public DroneDef def;
        public List<DroneComp> comps;

        public Pawn pawn;
        public Hediff_DroneHandler handlerHediff;
        public ThingWithComps equipmentSource;

        // When set to false, drone won't be rendered and drone comps won't have ActiveTick method called
        public bool active = true;

        public float health = 0;
        public List<DroneGraphicPackage> additionalGraphics;

        public static readonly Texture2D cachedPaletteTex = ContentFinder<Texture2D>.Get("UI/Gizmos/ColorPalette");
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public bool usePrimary;
        public bool useSecondary;
        public Command_Action paletteAction;

        public LocalTargetInfo currentTarget;

        public string Label
        {
            get
            {
                return def.label;
            }
        }

        public string LabelCap
        {
            get
            {
                return Label.CapitalizeFirst(def);
            }
        }

        public virtual List<StatModifier> StatOffsets
        {
            get
            {
                return def.ownerStatOffsets;
            }
        }

        public virtual List<StatModifier> StatFactors
        {
            get
            {
                return def.ownerStatFactors;
            }
        }

        public virtual float MaxHealth
        {
            get
            {
                return def.maxHealth;
            }
        }

        public virtual float RepairPerTick
        {
            get
            {
                return def.repairPerTick;
            }
        }

        public virtual float EnemyDetectionRange
        {
            get
            {
                return def.enemyDetectionRange;
            }
        }

        public virtual float HitInterceptChance
        {
            get
            {
                return def.hitInterceptChance;
            }
        }

        public virtual float AttackRangeMultiplier
        {
            get
            {
                return 1f;
            }
        }

        public bool InCombat
        {
            get
            {
                return PawnUtility.EnemiesAreNearby(pawn, 9, passDoors: true, EnemyDetectionRange, 1);
            }
        }

        public virtual Color PrimaryColor
        {
            get
            {
                return primaryColor;
            }

            set
            {
                primaryColor = value;
            }
        }

        public virtual Color SecondaryColor
        {
            get
            {
                return secondaryColor;
            }

            set
            {
                secondaryColor = value;
            }
        }

        public virtual LocalTargetInfo CurrentTarget
        {
            get
            {
                if (currentTarget != null && currentTarget.IsValid)
                {
                    return currentTarget;
                }

                currentTarget = null;
                float targetPriority = 0f;

                if (comps != null)
                {
                    for (int i = comps.Count - 1; i >= 0; i--)
                    {
                        (LocalTargetInfo, float) returnedTarget = comps[i].GetNewTarget();

                        if (returnedTarget.Item1 != null && returnedTarget.Item2 > targetPriority)
                        {
                            currentTarget = returnedTarget.Item1;
                            targetPriority = returnedTarget.Item2;
                        }
                    }
                }

                return currentTarget;
            }

            set
            {
                currentTarget = value;
            }
        }

        public virtual ThingWithComps EquipmentSource
        {
            get
            {
                return equipmentSource;
            }
        }

        public virtual bool UseSecondary => useSecondary;

        public Drone()
        {
        }

        public Drone(DroneDef def)
        {
            this.def = def;
            health = def.maxHealth;
            Initialize();
        }

        public Drone(Pawn pawn, DroneDef def)
        {
            this.pawn = pawn;
            this.def = def;
            health = def.maxHealth;
            Initialize();

            AthenaCache.AddCache(this, AthenaCache.responderCache, pawn.thingIDNumber);
        }

        public virtual void Initialize()
        {
            InitializeComps();
        }

        public virtual void InitializeComps()
        {
            if (def.comps.Any())
            {
                comps = new List<DroneComp>();
                for (int i = 0; i < def.comps.Count; i++)
                {
                    DroneComp comp = null;
                    try
                    {
                        comp = Activator.CreateInstance(def.comps[i].compClass) as DroneComp;
                        comp.parent = this;
                        comps.Add(comp);
                        comp.Initialize(def.comps[i]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Could not instantiate or initialize a DroneComp: " + ex);
                        comps.Remove(comp);
                    }
                }
            }
        }

        public T TryGetComp<T>() where T : DroneComp
        {
            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    if (comps[i] is T result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        public float TryGetStat(StatDef stat)
        {
            for (int i = def.statBases.Count - 1; i >= 0; i--)
            {
                StatModifier statMod = def.statBases[i];

                if (statMod.stat == stat)
                {
                    return statMod.value;
                }
            }

            return 0;
        }

        public virtual void ExposeData()
        {
            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                InitializeComps();
                AthenaCache.AddCache(this, AthenaCache.responderCache, pawn.thingIDNumber);
            }

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].CompExposeData();
                }
            }
        }

        public virtual void Tick()
        {
            if (active)
            {
                ActiveTick();
            }

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].Tick();
                }
            }

            HandleRepairs();
        }

        public virtual void ActiveTick()
        {
            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].ActiveTick();
                }
            }
        }

        public virtual void Deploy()
        {
            active = true;
            AthenaCache.AddCache(this, AthenaCache.responderCache, pawn.thingIDNumber);

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].OnDeployed();
                }
            }
        }

        public virtual void Recall()
        {
            active = false;
            AthenaCache.RemoveCache(this, AthenaCache.responderCache, pawn.thingIDNumber);

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].OnRecalled();
                }
            }
        }

        public virtual void ResetTarget()
        {
            currentTarget = null;
            float targetPriority = 0f;

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    (LocalTargetInfo, float) returnedTarget = comps[i].GetNewTarget();

                    if (returnedTarget.Item1 != null && returnedTarget.Item2 > targetPriority)
                    {
                        currentTarget = returnedTarget.Item1;
                        targetPriority = returnedTarget.Item2;
                    }
                }
            }
        }

        public virtual void PreApplyDamage(ref DamageInfo dinfo, ref bool absorbed)
        {
            float hitChance = HitInterceptChance;

            if (def.directionalHitChanceMultipliers != null)
            {
                Rot4 hitDir = Rot4.North;

                if (dinfo.Instigator != null && dinfo.Instigator.PositionHeld.DistanceToSquared(pawn.PositionHeld) > 1.9f)
                {
                    Vector3 attackVector = (dinfo.Instigator.PositionHeld - pawn.PositionHeld).ToVector3().normalized;
                    if (Math.Abs(attackVector.x) > Math.Abs(attackVector.z))
                    {
                        if (attackVector.x > 0)
                        {
                            hitDir = Rot4.East;
                        }
                        else
                        {
                            hitDir = Rot4.West;
                        }
                    }
                    else
                    {
                        if (attackVector.z > 0)
                        {
                            hitDir = Rot4.North;
                        }
                        else
                        {
                            hitDir = Rot4.South;
                        }
                    }
                }
                else
                {
                    hitDir = Rot4.FromAngleFlat(dinfo.Angle);
                }

                hitChance *= def.directionalHitChanceMultipliers[(int)Rot4.GetRelativeRotation(pawn.Rotation, hitDir)];
            }

            if (!Rand.Chance(hitChance))
            {
                return;
            }

            float damAmount = dinfo.Amount;
            float armorRating = TryGetStat(dinfo.Def.armorCategory.armorRatingStat);

            float armor = Mathf.Max(armorRating - dinfo.ArmorPenetrationInt, 0f);
            float randValue = Rand.Value;

            if (randValue < armor * 0.5f)
            {
                absorbed = true;
                return;
            }
            else if (randValue < armor)
            {
                damAmount = GenMath.RoundRandom(damAmount / 2f);
            }

            dinfo.SetAmount(damAmount);
            ApplyDamage(dinfo, ref absorbed);
        }

        public virtual void ApplyDamage(DamageInfo dinfo, ref bool absorbed)
        {
            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].PreApplyDamage(dinfo);
                }
            }

            health -= dinfo.Amount;

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].PostApplyDamage(dinfo);
                }
            }

            if (health > 0)
            {
                absorbed = true;
                return;
            }

            dinfo.SetAmount(-health);
            Destroy();
        }

        public virtual void Destroy()
        {
            pawn.health.RemoveHediff(handlerHediff);
            handlerHediff = null;
        }

        public virtual void OnDestroyed()
        {
            AthenaCache.RemoveCache(this, AthenaCache.responderCache, pawn.thingIDNumber);

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].Destroyed();
                }
            }
        }

        public virtual void HandleRepairs()
        {
            switch (def.repairType)
            {
                case DroneRepairType.Passive:
                    health += RepairPerTick;
                    break;

                case DroneRepairType.OutOfCombat:

                    if (!InCombat)
                    {
                        health += RepairPerTick;
                    }

                    break;

                case DroneRepairType.Recalled:

                    if (!active)
                    {
                        health += RepairPerTick;
                    }

                    break;

                case DroneRepairType.RecalledOutOfCombat:

                    if (!InCombat && !active)
                    {
                        health += RepairPerTick;
                    }

                    break;
            }

            health = Math.Max(health, MaxHealth);
        }

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
            DrawSecondaries(drawPos, bodyType);

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].DrawAt(drawPos, bodyType);
                }
            }
        }

        public virtual void DrawSecondaries(Vector3 drawPos, BodyTypeDef bodyType)
        {
            if (def.additionalGraphics == null)
            {
                return;
            }

            if (additionalGraphics == null)
            {
                RecacheGraphicData();
            }

            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                DroneGraphicPackage package = additionalGraphics[i];
                Vector3 offset = new Vector3();

                if (package.onlyRenderWhenDrafted && (pawn.drafter == null || !pawn.drafter.Drafted))
                {
                    return;
                }

                if (package.offsets != null)
                {
                    if (package.offsets.Count == 4)
                    {
                        offset = package.offsets[pawn.Rotation.AsInt];
                    }
                    else
                    {
                        offset = package.offsets[0];
                    }
                }

                package.GetGraphic(this).Draw(drawPos + offset, pawn.Rotation, pawn);
            }
        }

        public virtual void RecacheGraphicData()
        {
            if (def.additionalGraphics == null)
            {
                additionalGraphics = new List<DroneGraphicPackage>();
            }
            else
            {
                additionalGraphics = new List<DroneGraphicPackage>(def.additionalGraphics);
            }

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    additionalGraphics = additionalGraphics.Concat(comps[i].GetAdditionalGraphics()).ToList();
                }
            }

            for (int i = additionalGraphics.Count - 1; i >= 0; i++)
            {
                DroneGraphicPackage package = additionalGraphics[i];
                
                if (package.firstMask == DronePackageColor.PrimaryColor || package.secondMask == DronePackageColor.PrimaryColor)
                {
                    usePrimary = true;
                }

                if (package.firstMask == DronePackageColor.SecondaryColor || package.secondMask == DronePackageColor.SecondaryColor)
                {
                    usePrimary = true;
                    useSecondary = true;
                }
            }
        }

        public virtual IEnumerable<Gizmo> GetGizmosExtra()
        {
            if (usePrimary)
            {
                if (paletteAction == null)
                {
                    paletteAction = new Command_Action();
                    paletteAction.defaultLabel = "Change colors for " + LabelCap;
                    paletteAction.icon = cachedPaletteTex;
                    paletteAction.action = delegate ()
                    {
                        Find.WindowStack.Add(new Dialog_ColorPalette(this));
                    };
                }

                yield return paletteAction;
            }

            if (comps == null)
            {
                yield break;
            }

            foreach (DroneComp comp in comps)
            {
                foreach (Gizmo item in comp.CompGetGizmosExtra())
                {
                    yield return item;
                }
            }
        }

        public virtual float GetHitChance(float distance)
        {
            return AthenaCombatUtility.DistanceToAccuracy(distance, TryGetStat(StatDefOf.AccuracyTouch), TryGetStat(StatDefOf.AccuracyShort), TryGetStat(StatDefOf.AccuracyMedium), TryGetStat(StatDefOf.AccuracyLong));
        }
    }
}
