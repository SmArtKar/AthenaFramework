using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public static class AthenaArmor
    {
        public static bool CoversBodyPart(Thing armor, ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn)
        {
            bool defaultCover = armor.def.apparel.CoversBodyPart(part);
            bool currentCover = defaultCover;

            bool haveForceCover = false;
            bool haveForceUncover = false;

            if (!AthenaCache.armorCache.TryGetValue(armor.thingIDNumber, out List<IArmored> mods))
            {
                return defaultCover;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                if (mods[i].CoversBodypart(ref amount, armorPenetration, stat, ref armorRating, part, ref damageDef, pawn, defaultCover, out bool forceCover, out bool forceUncover))
                {
                    currentCover = true;
                }

                if (forceCover)
                {
                    haveForceCover = true;
                }

                if (forceUncover)
                {
                    haveForceUncover = true;
                }
            }

            if (haveForceCover)
            {
                return true;
            }

            if (haveForceUncover)
            {
                return false;
            }

            return currentCover;
        }

        public static void ApplyArmor(Thing armor, ref float amount, float armorPenetration, StatDef stat, float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, out bool metalArmor)
        {
            DamageDef originalDamageDef = damageDef;
            float originalAmount = amount;

            bool foundMods = AthenaCache.armorCache.TryGetValue((armor ?? pawn).thingIDNumber, out List<IArmored> mods);
            metalArmor = false;

            if (foundMods)
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    if (!mods[i].PreProcessArmor(ref amount, armorPenetration, stat, ref armorRating, part, ref damageDef, pawn, ref metalArmor, originalDamageDef, originalAmount))
                    {
                        return;
                    }
                }
            }

            ArmorUtility.ApplyArmor(ref amount, armorPenetration, armorRating, armor, ref damageDef, pawn, out bool isMetalArmor);

            if (isMetalArmor)
            {
                metalArmor = true;
            }

            if (foundMods)
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    mods[i].PostProcessArmor(ref amount, armorPenetration, stat, ref armorRating, part, ref damageDef, pawn, ref metalArmor, originalDamageDef, originalAmount);
                }
            }
        }

        public static void DamageModification(Verb verb, ref float damage, ref float armorPenetration, ref LocalTargetInfo target)
        {
            AdvancedTool advTool = verb.tool as AdvancedTool;

            if (advTool != null)
            {
                advTool.DamageModification(verb, ref damage, ref armorPenetration, ref target, verb.CasterPawn);
            }
        }

        public static float GetHitChance(IntVec3 aimPosition, Thing target, HitChanceFlags hitFlags, Thing caster = null)
        {
            float hitChance = 1f;
            float offset = 0f;
            float distance = target.Position.DistanceTo(aimPosition);

            if ((hitFlags & HitChanceFlags.Gas) != 0)
            {
                List<IntVec3> lineOfSight = GenSight.PointsOnLineOfSight(aimPosition, target.Position).ToList();

                for (int i = 0; i < lineOfSight.Count; i++)
                {
                    if (lineOfSight[i].AnyGas(target.Map, GasType.BlindSmoke))
                    {
                        hitChance *= 0.7f;
                        break;
                    }
                }
            }

            if ((hitFlags & HitChanceFlags.Weather) != 0)
            {
                if ((caster != null && !caster.Position.Roofed(caster.Map)) || !target.Position.Roofed(target.Map))
                {
                    hitChance *= target.Map.weatherManager.CurWeatherAccuracyMultiplier;
                }
            }

            if ((hitFlags & HitChanceFlags.Lighting) != 0)
            {
                if (ModsConfig.IdeologyActive && caster != null)
                {
                    if (DarknessCombatUtility.IsOutdoorsAndLit(target))
                    {
                        offset += caster.GetStatValue(StatDefOf.ShootingAccuracyOutdoorsLitOffset);
                    }
                    else if (DarknessCombatUtility.IsOutdoorsAndDark(target))
                    {
                        offset += caster.GetStatValue(StatDefOf.ShootingAccuracyOutdoorsDarkOffset);
                    }
                    else if (DarknessCombatUtility.IsIndoorsAndDark(target))
                    {
                        offset += caster.GetStatValue(StatDefOf.ShootingAccuracyIndoorsDarkOffset);
                    }
                    else if (DarknessCombatUtility.IsIndoorsAndLit(target))
                    {
                        offset += caster.GetStatValue(StatDefOf.ShootingAccuracyIndoorsLitOffset);
                    }
                }
            }

            Pawn pawn = target as Pawn;

            if ((hitFlags & HitChanceFlags.Size) != 0)
            {
                if (pawn != null)
                {
                    hitChance *= pawn.BodySize;
                }
                else
                {
                    hitChance *= target.def.fillPercent * target.def.size.x * target.def.size.z * 2.5f;
                }
            }

            if ((hitFlags & HitChanceFlags.ShooterStats) != 0)
            {
                float statValue = 1f;

                if (caster is Pawn)
                {
                    statValue = caster.GetStatValue(StatDefOf.ShootingAccuracyPawn);
                    statValue = ((distance <= 3f) ? (statValue * caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Touch)) : ((distance <= 12f) ? (statValue * Mathf.Lerp(caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Touch), caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Short), (distance - 3f) / 12f - 3f)) : ((distance <= 25f) ? (statValue * Mathf.Lerp(caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Short), caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Medium), (distance - 12f) / 25f - 12f)) : ((!(distance <= 40f)) ? (statValue * caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Long)) : (statValue * Mathf.Lerp(caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Medium), caster.GetStatValue(StatDefOf.ShootingAccuracyFactor_Long), (distance - 25f) / 40f - 25f))))));
                }
                else
                {
                    statValue = caster.GetStatValue(StatDefOf.ShootingAccuracyTurret);
                }

                hitChance *= Mathf.Max(Mathf.Pow(statValue, distance), 0.0201f);
            }

            if (pawn == null)
            {
                return Math.Max(0.0201f, hitChance + offset);
            }

            if ((hitFlags & HitChanceFlags.Posture) != 0 && distance >= 4.5f && pawn.GetPosture() != 0)
            {
                hitChance *= 0.2f;
            }

            if ((hitFlags & HitChanceFlags.Execution) != 0 && distance <= 3.9f && pawn.GetPosture() != 0)
            {
                hitChance *= 7.5f;
            }

            return Math.Max(0.0201f, hitChance + offset);
        }
    }
    
    [Flags]
    public enum HitChanceFlags
    {
        None = 0,
        Posture = 1,
        Gas = 2,
        Weather = 4,
        Lighting = 8,
        Size = 16,
        Execution = 32,
        ShooterStats = 64,
        All = -1
    }
}
