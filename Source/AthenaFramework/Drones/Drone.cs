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
    [StaticConstructorOnStartup]
    public class Drone : IRenderable, IColorSelector, IDamageResponse, IExposable
    {
        public DroneDef def;
        public List<DroneComp> comps;

        public Pawn pawn;
        public Hediff_DroneHandler handlerHediff;
        public ThingWithComps equipmentSource;
        public HediffWithComps hediffSource;

        // When set to false, drone won't be rendered and drone comps won't have ActiveTick method called
        public bool active = false;

        public float health = 0;
        public List<DroneGraphicPackage> additionalGraphics;

        // Set to true if the drone breaks instead of being destroyed upon death. Prevents the drone from being deployed until its health if fully restored
        public bool broken = false;

        public static readonly Texture2D cachedPaletteTex = ContentFinder<Texture2D>.Get("UI/Gizmos/ColorPalette");
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public bool usePrimary;
        public bool useSecondary;
        public Command_Action paletteAction;

        private LocalTargetInfo currentTarget;
        private float currentTargetPriority = -1f;

        public int lastTargetingTick;

        public Gizmo_DroneHealthStatus healthGizmo;

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

        public virtual bool InCombat
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

        public virtual ThingWithComps EquipmentSource
        {
            get
            {
                return equipmentSource;
            }
        }

        public virtual HediffWithComps HediffSource
        {
            get
            {
                return hediffSource;
            }
        }

        public virtual Graphic DroneGraphic
        {
            get
            {
                return def.graphicData.Graphic;
            }
        }

        public virtual bool UseSecondary => useSecondary;

        public virtual IntVec3 CurrentPosition
        {
            get
            {
                if (comps != null)
                {
                    for (int i = comps.Count - 1; i >= 0; i--)
                    {
                        IntVec3? overridePosition = comps[i].PositionOverride();

                        if (overridePosition != null)
                        {
                            return overridePosition.Value;
                        }
                    }
                }

                return pawn.PositionHeld;
            }
        }

        public virtual Vector3 DrawPos
        {
            get
            {
                Vector3 drawPos = pawn.DrawPos;
                drawPos.y = def.defaultLayer.AltitudeFor();

                if (comps != null)
                {
                    for (int i = comps.Count - 1; i >= 0; i--)
                    {
                        drawPos += comps[i].DrawPosOffset();

                        if (comps[i].DrawPosOverride(ref drawPos))
                        {
                            return drawPos;
                        }
                    }
                }

                return drawPos;
            }
        }

        public virtual LocalTargetInfo CurrentTarget
        {
            get
            {
                if (Find.TickManager.TicksGame > lastTargetingTick + def.targetRefreshRate || currentTargetPriority == -1f)
                {
                    RecacheTarget();
                }

                return currentTarget;
            }
        }

        public virtual float CurrentTargetPriority
        {
            get
            {
                return currentTargetPriority;
            }
        }

        public Drone() { }

        public Drone(Pawn pawn, DroneDef def)
        {
            this.pawn = pawn;
            this.def = def;
            health = def.maxHealth;
            Initialize();

            AthenaCache.AddCache(this, ref AthenaCache.responderCache, pawn.thingIDNumber);
        }

        public virtual void Initialize()
        {
            InitializeComps();
            SetupOnPawn();
        }

        public virtual void SetupOnPawn()
        {
            handlerHediff = pawn.health.AddHediff(AthenaDefOf.Athena_DroneHandler) as Hediff_DroneHandler;
            handlerHediff.drone = this;

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].OnSetup();
                }
            }

            Deploy();
        }

        public virtual void CleanRemove() // Used for when the drone is intended to be stored somewhere instead of destroyed
        {
            Recall();
            handlerHediff.CleanRemove();
            handlerHediff = null;
            pawn = null;
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

        public virtual void RecacheTarget()
        {
            if (comps == null)
            {
                return;
            }

            bool updatedTarget = false;

            for (int i = comps.Count - 1; i >= 0; i--)
            {
                if (comps[i].RecacheTarget(out LocalTargetInfo newTarget, out float newPriority))
                {
                    SetTarget(newTarget, newPriority);
                    updatedTarget = true;
                }
            }

            if (!updatedTarget)
            {
                SetTarget(null, -1f); // If we were unable to locate a new target, wipe the existing one.
            }
        }

        public T TryGetComp<T>() where T : DroneComp
        {
            if (comps == null)
            {
                return null;
            }

            for (int i = comps.Count - 1; i >= 0; i--)
            {
                if (comps[i] is T result)
                {
                    return result;
                }
            }

            return null;
        }

        public virtual float TryGetStat(StatDef stat)
        {
            for (int i = def.statBases.Count - 1; i >= 0; i--)
            {
                StatModifier statMod = def.statBases[i];

                if (statMod.stat == stat)
                {
                    if (comps == null)
                    {
                        return statMod.value;
                    }

                    float value = statMod.value;

                    for (int j = comps.Count - 1; j >= 0; j--)
                    {
                        comps[i].TryGetStat(stat, ref value);
                    }

                    return value;
                }
            }

            return 0;
        }

        public virtual void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_References.Look(ref equipmentSource, "equipmentSource");
            Scribe_References.Look(ref hediffSource, "hediffSource");
            Scribe_References.Look(ref pawn, "pawn");
            Scribe_References.Look(ref handlerHediff, "handlerHediff");
            Scribe_Values.Look(ref active, "active");
            Scribe_Values.Look(ref health, "health");

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs && active)
            {
                InitializeComps();
                AthenaCache.AddCache(this, ref AthenaCache.responderCache, pawn.thingIDNumber);
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
            if (active && pawn.Spawned)
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
            if (broken)
            {
                return;
            }

            active = true;
            AthenaCache.AddCache(this, ref AthenaCache.responderCache, pawn.thingIDNumber);

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

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].PrePawnApplyDamage(ref dinfo, ref hitChance, ref absorbed);

                    if (absorbed)
                    {
                        return;
                    }
                }
            }

            if (CurrentPosition != pawn.Position) // Cannot block if we're mid ranged attack
            {
                hitChance = 0;
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

            if (def.deathEffecter != null)
            {
                def.deathEffecter.SpawnAttached(pawn, pawn.Map).Cleanup();
            }

            if (Rand.Chance(def.destroyChance))
            {
                Destroy();
                return;
            }

            health = 0;
            broken = true;
            Recall();
        }

        public virtual void Destroy()
        {
            Recall();
            pawn.health.RemoveHediff(handlerHediff);
        }

        public virtual void OnDestroyed()
        {
            AthenaCache.RemoveCache(this, AthenaCache.responderCache, pawn.thingIDNumber);
            handlerHediff = null;
            pawn = null;

            if (comps != null)
            {
                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    comps[i].OnDestroyed();
                }
            }
        }

        public virtual void RepairDrone(float amount, bool allowOverhealth = false)
        {
            health += amount;

            if (!allowOverhealth)
            {
                health = Math.Max(health, MaxHealth);
            }

            if (broken && health >= MaxHealth)
            {
                broken = false;
            }
        }

        public virtual bool DisableHoveringAnimation()
        {

            if (comps == null)
            {
                return false;
            }

            for (int i = comps.Count - 1; i >= 0; i--)
            {
                if (comps[i].DisableHoveringAnimation())
                {
                    return true;
                }
            }

            return false;
        }

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
            drawPos += DrawPos - pawn.DrawPos; //Gets purely offsets without the initial draw pos

            DroneGraphic.Draw(drawPos, pawn.Rotation, pawn);

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
            if (additionalGraphics == null)
            {
                RecacheGraphicData();
            }

            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                DroneGraphicPackage package = additionalGraphics[i];

                if (!package.CanRender(this))
                {
                    continue;
                }

                Vector3 offset = new Vector3();

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

        public virtual IEnumerable<Gizmo> GetGizmos()
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

            if (def.healthDisplay)
            {
                if (healthGizmo == null)
                {
                    healthGizmo = new Gizmo_DroneHealthStatus(this);
                }

                yield return healthGizmo;
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

        public virtual float GetRangedHitChance(float distance)
        {
            return AthenaCombatUtility.DistanceToAccuracy(distance, TryGetStat(StatDefOf.AccuracyTouch), TryGetStat(StatDefOf.AccuracyShort), TryGetStat(StatDefOf.AccuracyMedium), TryGetStat(StatDefOf.AccuracyLong));
        }

        public virtual bool SetTarget(LocalTargetInfo newTarget, float targetPriority = -1f)
        {
            if (targetPriority != -1 && targetPriority < currentTargetPriority)
            {
                return false;
            }

            currentTargetPriority = targetPriority;

            if (currentTarget == newTarget)
            {
                return true;
            }

            currentTarget = newTarget;

            if (comps == null)
            {
                return true;
            }

            for (int i = comps.Count - 1; i >= 0; i--)
            {
                comps[i].TargetUpdate();
            }

            return true;
        }
    }
}
