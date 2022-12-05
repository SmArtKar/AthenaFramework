using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace AthenaFramework
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.smartkar.athenaframework.main");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(DamageInfo), nameof(DamageInfo.Amount), MethodType.Getter)]
        public static class DamageInfo_AmountGetter
        {
            static void Postfix(DamageInfo __instance, float __result)
            {
                if (__instance.Instigator == null || !(__instance.Instigator is Pawn))
                {
                    return;
                }

                Pawn pawn = __instance.Instigator as Pawn;
                if (AthenaHediffUtility.amplifierCompsByPawn.ContainsKey(pawn))
                {
                    foreach (CompHediff_DamageAmplifier amplifier in AthenaHediffUtility.amplifierCompsByPawn[pawn])
                    {
                        __result *= amplifier.damageMultiplier;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt))]
        public static class Pawn_PostDrawAt
        {
            static void Postfix(Pawn __instance, Vector3 drawLoc)
            {
                if (AthenaHediffUtility.renderableCompsByPawn.ContainsKey(__instance))
                {
                    foreach (CompHediff_Renderable renderable in AthenaHediffUtility.renderableCompsByPawn[__instance])
                    {
                        renderable.DrawAt(drawLoc);
                    }
                }
            }
        }
    }
}
