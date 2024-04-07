using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace AthenaFramework
{
    public static class AthenaCombatUtility
    {

        #region ===== Armor Calculations =====

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

        #endregion

        #region ===== Tool Utilities =====

        public static void ModifyToolDamage(Verb verb, ref DamageInfo dinfo, LocalTargetInfo target, out IEnumerator<DamageInfo> additionalDamage)
        {
            if (verb.tool == null)
            {
                additionalDamage = null;
                return;
            }

            AdvancedTool advTool = verb.tool as AdvancedTool;

            if (advTool == null)
            {
                additionalDamage = null;
                return;
            }

            advTool.DamageModification(verb, ref dinfo, target, verb.CasterPawn, out additionalDamage);
        }

        public static void ModifyToolTarget(Verb verb, ref LocalTargetInfo target)
        {
            if (verb.tool == null)
            {
                return;
            }

            AdvancedTool advTool = verb.tool as AdvancedTool;

            if (advTool == null)
            {
                return;
            }

            advTool.TargetModification(verb, ref target);
        }

        public static void Cum(LocalTargetInfo target, Verb verb)
        {
            ModifyToolTarget(verb, ref target);
        }

        #endregion

        #region ===== Ranged Weapon Calculations =====

        public static float GetRangedHitChance(IntVec3 aimPosition, Thing target, HitChanceFlags hitFlags, Thing caster = null)
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
                if (!aimPosition.Roofed(caster.Map) || !target.Position.Roofed(target.Map))
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

        public static float DistanceToAccuracy(float dist, float touchAcc, float shortAcc, float medAcc, float longAcc)
        {
            if (dist <= 3)
            {
                return touchAcc;
            }

            if (dist <= 12f)
            {
                return Mathf.Lerp(touchAcc, shortAcc, (dist - 3f) / 9f);
            }

            if (dist <= 25f)
            {
                return Mathf.Lerp(shortAcc, medAcc, (dist - 12f) / 13f);
            }

            if (dist <= 40f)
            {
                return Mathf.Lerp(medAcc, longAcc, (dist - 25f) / 15f);
            }

            return longAcc;
        }

        public static Thing GetRandomCoverToMissInto(LocalTargetInfo target, IntVec3 firerPos, Map map)
        {
            List<CoverInfo> covers = CoverUtility.CalculateCoverGiverSet(target, firerPos, map);

            if (covers.TryRandomElementByWeight((CoverInfo c) => c.BlockChance, out var result))
            {
                return result.Thing;
            }

            return null;
        }

        #endregion

        #region ===== Shoot Lines =====

        public static bool GetShootLine(IntVec3 root, Map map, LocalTargetInfo target, out ShootLine resultingLine, bool canLean = true, CellRect? occupiedRect = null)
        {
            if (target.HasThing && target.Thing.Map != map)
            {
                resultingLine = default(ShootLine);
                return false;
            }

            IntVec3 goodDest;

            if (occupiedRect != null)
            {
                foreach (IntVec3 item in occupiedRect)
                {
                    if (CanHitFromCellIgnoringRange(item, target, map, out goodDest))
                    {
                        resultingLine = new ShootLine(item, goodDest);
                        return true;
                    }
                }
            }

            if (CanHitFromCellIgnoringRange(root, target, map, out goodDest))
            {
                resultingLine = new ShootLine(root, goodDest);
                return true;
            }

            if (canLean)
            {
                List<IntVec3> leanShootSources = new List<IntVec3>();
                ShootLeanUtility.LeanShootingSourcesFromTo(root, occupiedRect == null ? root : occupiedRect.Value.ClosestCellTo(root), map, leanShootSources);

                for (int i = 0; i < leanShootSources.Count; i++)
                {
                    IntVec3 intVec = leanShootSources[i];
                    if (CanHitFromCellIgnoringRange(intVec, target, map, out goodDest))
                    {
                        resultingLine = new ShootLine(intVec, goodDest);
                        return true;
                    }
                }
            }

            resultingLine = new ShootLine(root, target.Cell);
            return false;
        }


        public static bool CanHitFromCellIgnoringRange(IntVec3 sourceCell, LocalTargetInfo target, Map map, out IntVec3 goodDest)
        {
            if (target.Thing != null)
            {
                if (target.Thing.Map != map)
                {
                    goodDest = IntVec3.Invalid;
                    return false;
                }

                List<IntVec3> tempDestList = new List<IntVec3>();

                ShootLeanUtility.CalcShootableCellsOf(tempDestList, target.Thing, sourceCell);
                for (int i = 0; i < tempDestList.Count; i++)
                {
                    if (CanHitCellFromCellIgnoringRange(sourceCell, tempDestList[i], map, target.Thing.def.Fillage == FillCategory.Full))
                    {
                        goodDest = tempDestList[i];
                        return true;
                    }
                }
            }
            else if (CanHitCellFromCellIgnoringRange(sourceCell, target.Cell, map))
            {
                goodDest = target.Cell;
                return true;
            }

            goodDest = IntVec3.Invalid;
            return false;
        }

        public static bool CanHitCellFromCellIgnoringRange(IntVec3 sourceSq, IntVec3 targetLoc, Map map, bool includeCorners = false, bool mustCastOnOpenGround = false)
        {
            if (mustCastOnOpenGround && (!targetLoc.Standable(map) || map.thingGrid.CellContains(targetLoc, ThingCategory.Pawn)))
            {
                return false;
            }

            if (!includeCorners)
            {
                if (!GenSight.LineOfSight(sourceSq, targetLoc, map, skipFirstCell: true))
                {
                    return false;
                }
            }
            else if (!GenSight.LineOfSightToEdges(sourceSq, targetLoc, map, skipFirstCell: true))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region ===== Angular Shotguns =====

        public static Thing GetPelletTarget(float angle, float range, IntVec3 shooterPosition, Map map, IntVec3 target, out IntVec3 rangeEndPosition, Thing caster = null, Verb verb = null, HitChanceFlags hitFlags = HitChanceFlags.None)
        {
            IntVec3 endPosition = (new IntVec3((int)(Math.Cos(angle) * range), 0, (int)(Math.Sin(angle) * range)));
            if (target.z < shooterPosition.z)
            {
                endPosition.z *= -1;
            }
            rangeEndPosition = endPosition + shooterPosition;
            Thing newTarget = null;

            List<IntVec3> points = GenSight.PointsOnLineOfSight(shooterPosition, rangeEndPosition).Concat(rangeEndPosition).ToList();

            for (int j = 0; j < points.Count; j++)
            {
                IntVec3 targetPosition = points[j];

                if (!targetPosition.IsValid)
                {
                    continue;
                }

                if (targetPosition == shooterPosition) //Prevents the caster from shooting himself
                {
                    continue;
                }

                Thing targetBuilding = targetPosition.GetRoofHolderOrImpassable(map);
                if (targetBuilding != null)
                {
                    newTarget = targetBuilding;
                    break;
                }

                Thing cover = targetPosition.GetCover(map);
                if (cover != null)
                {
                    if (Rand.Chance(cover.BaseBlockChance()))
                    {
                        newTarget = targetBuilding;
                        break;
                    }
                }

                List<Thing> thingList = GridsUtility.GetThingList(targetPosition, map);
                for (int k = thingList.Count - 1; k >= 0; k--)
                {
                    Pawn pawnTarget = thingList[k] as Pawn;

                    if (pawnTarget == null)
                    {
                        continue;
                    }

                    if (caster != null && verb != null)
                    {
                        ShotReport shotReport = ShotReport.HitReportFor(caster, verb, pawnTarget);
                        if (Rand.Chance(shotReport.AimOnTargetChance))
                        {
                            newTarget = pawnTarget;
                            break;
                        }
                    }
                    else
                    {
                        if (Rand.Chance(GetRangedHitChance(shooterPosition, pawnTarget, hitFlags, caster)))
                        {
                            newTarget = pawnTarget;
                            break;
                        }
                    }
                }

                if (newTarget != null)
                {
                    break;
                }
            }

            return newTarget;
        }

        #endregion

        #region ===== Melee Weapon Calculations =====

        public static float GetDodgeChance(LocalTargetInfo target)
        {
            if (IsTargetImmobile(target))
            {
                return 0f;
            }

            Pawn pawn = target.Pawn;

            if (pawn == null)
            {
                return 0f;
            }

            if (pawn.stances.curStance is Stance_Busy stance_Busy && stance_Busy.verb != null && !stance_Busy.verb.verbProps.IsMeleeAttack)
            {
                return 0f;
            }

            float dodgeChance = pawn.GetStatValue(StatDefOf.MeleeDodgeChance);

            if (ModsConfig.IdeologyActive)
            {
                if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
                {
                    dodgeChance += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsLitOffset);
                }
                else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
                {
                    dodgeChance += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceOutdoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
                {
                    dodgeChance += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
                {
                    dodgeChance += pawn.GetStatValue(StatDefOf.MeleeDodgeChanceIndoorsLitOffset);
                }
            }

            return dodgeChance;
        }

        public static bool IsTargetImmobile(LocalTargetInfo target)
        {
            Thing thing = target.Thing;
            Pawn pawn = thing as Pawn;

            if (thing.def.category == ThingCategory.Pawn && !pawn.Downed)
            {
                return pawn.GetPosture() != PawnPosture.Standing;
            }

            return true;
        }

        public static void ApplyMeleeSlaveSuppression(Pawn attacker, Pawn target, float damageDealt)
        {
            if (!attacker.IsColonist || attacker.IsSlave)
            {
                return;
            }

            if (!target.IsSlaveOfColony || !target.health.capacities.CanBeAwake)
            {
                return;
            }

            SlaveRebellionUtility.IncrementMeleeSuppression(attacker, target, damageDealt);
        }

        public static float GetMeleeHitChance(Pawn attacker, LocalTargetInfo target)
        {
            if (IsTargetImmobile(target))
            {
                return 1f;
            }

            float num = attacker.GetStatValue(StatDefOf.MeleeHitChance);

            if (ModsConfig.IdeologyActive && target.HasThing)
            {
                if (DarknessCombatUtility.IsOutdoorsAndLit(target.Thing))
                {
                    num += attacker.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsLitOffset);
                }
                else if (DarknessCombatUtility.IsOutdoorsAndDark(target.Thing))
                {
                    num += attacker.GetStatValue(StatDefOf.MeleeHitChanceOutdoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndDark(target.Thing))
                {
                    num += attacker.GetStatValue(StatDefOf.MeleeHitChanceIndoorsDarkOffset);
                }
                else if (DarknessCombatUtility.IsIndoorsAndLit(target.Thing))
                {
                    num += attacker.GetStatValue(StatDefOf.MeleeHitChanceIndoorsLitOffset);
                }
            }
            return num;
        }

        #endregion

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
