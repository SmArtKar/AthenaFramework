using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
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
}
