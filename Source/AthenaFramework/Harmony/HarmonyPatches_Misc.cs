using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    [HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.IsAcceptablePreyFor))]
    public static class FoodUtility_PreAcceptablePrey
    {
        public static void Postfix(Pawn predator, Pawn prey, ref bool __result)
        {
            if (!__result)
            {
                return;
            }

            MinPreySizeExtension ext = predator.def.GetModExtension<MinPreySizeExtension>();

            if (ext != null && prey.BodySize < ext.minPreyBodySize)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.SpawnSetup))]
    public static class Pawn_PostSpawnSetup
    {
        public static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
        {
            if (respawningAfterLoad || __instance.def.GetModExtension<HediffGiverExtension>() == null)
            {
                return;
            }

            HediffGiverExtension extension = __instance.def.GetModExtension<HediffGiverExtension>();

            for (int i = extension.bodypartPairs.Count - 1; i >= 0; i--)
            {
                HediffBodypartPair pair = extension.bodypartPairs[i];

                if (pair.bodyPartDef == null)
                {
                    Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, __instance, null);
                    __instance.health.AddHediff(hediff, null, null, null);
                    continue;
                }

                List<BodyPartRecord> partRecords = __instance.RaceProps.body.GetPartsWithDef(pair.bodyPartDef);
                for (int j = partRecords.Count - 1; j >= 0; j--)
                {
                    BodyPartRecord partRecord = partRecords[j];
                    Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, __instance, partRecord);
                    __instance.health.AddHediff(hediff, partRecord, null, null);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Recipe_Surgery), nameof(Recipe_Surgery.AvailableOnNow))]
    public static class Recipe_Surgery_AvailibilityPatch
    {
        public static void Postfix(Recipe_Surgery __instance, Thing thing, BodyPartRecord part, ref bool __result)
        {
            if (!__result || __instance.recipe.addsHediff == null)
            {
                return;
            }

            Pawn pawn = thing as Pawn;

            if (pawn != null)
            {
                return;
            }

            HediffDef def = __instance.recipe.addsHediff;

            if (def.comps == null)
            {
                return;
            }

            for (int i = def.comps.Count - 1; i >= 0; i--)
            {
                HediffCompProperties_PerquisiteHediff comp = def.comps[i] as HediffCompProperties_PerquisiteHediff;

                if (comp == null)
                {
                    continue;
                }

                if (!comp.ValidSurgery(__instance, pawn, part))
                {
                    __result = false;
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.LoadGame))]
    public static class Game_PostLoad
    {
        public static void Prefix()
        {
            AthenaCache.Reset();
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.InitNewGame))]
    public static class Game_PostInit
    {
        public static void Prefix()
        {
            AthenaCache.Reset();
        }
    }

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.StatOffsetFromGear))]
    public static class StatWorker_GearOffset
    {
        public static void Postfix(StatWorker __instance, Thing gear, StatDef stat, ref float __result)
        {
            if (gear == null || stat == null || AthenaCache.statmodCache == null)
            {
                return;
            }

            if (!AthenaCache.statmodCache.TryGetValue(gear.thingIDNumber, out List<IStatModifier> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                mods[i].GearStatOffset(stat, ref __result);
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GearHasCompsThatAffectStat))]
    public static class StatWorker_GearAffects
    {
        public static void Postfix(StatWorker __instance, Thing gear, StatDef stat, ref bool __result)
        {
            if (__result || gear == null || AthenaCache.statmodCache == null)
            {
                return;
            }

            if (!AthenaCache.statmodCache.TryGetValue(gear.thingIDNumber, out List<IStatModifier> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                if(mods[i].GearAffectsStat(stat))
                {
                    __result = true;
                    return;
                }
            }
        }
    }

    [HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetValueUnfinalized))]
    public static class StatWorker_GetValue
    {
        private static List<IStatModifier> mods;

        static StatWorker_GetValue()
        {
            mods = new List<IStatModifier>();
        }

        public static void Prefix(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result)
        {
            if (!req.HasThing || AthenaCache.statmodCache == null)
            {
                return;
            }

            mods = null;

            if (!AthenaCache.statmodCache.TryGetValue(req.Thing.thingIDNumber, out mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                mods[i].GetValueOffsets(__instance, req, applyPostProcess, ref __result);
            }
        }

        public static void Postfix(StatWorker __instance, StatRequest req, bool applyPostProcess, ref float __result)
        {
            if (!req.HasThing || mods == null)
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                mods[i].GetValueFactors(__instance, req, applyPostProcess, ref __result);
            }
        }
    }

    [HarmonyPatch]
    public static class EquipmentUtility_CanEquip
    {
        public static MethodBase TargetMethod()
        {
            return AccessTools.FirstMethod(typeof(EquipmentUtility), method => method.Name.Contains(nameof(EquipmentUtility.CanEquip)) && method.GetParameters().Count() == 4);
        }

        public static void Postfix(Thing thing, Pawn pawn, ref string cantReason, bool checkBonded, ref bool __result)
        {
            if (!AthenaCache.equipCache.TryGetValue(thing.thingIDNumber, out List<IPreventEquip> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                if (mods[i].PreventEquip(pawn, out string reason))
                {
                    __result = false;
                    cantReason = reason;
                    return;
                }
            }
        }
    }
}
