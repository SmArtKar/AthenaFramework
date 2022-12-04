using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

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
                foreach (HediffWithComps hediff in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>())
                {
                    foreach (CompHediff_DamageAmplifier amplifier in hediff.comps.OfType<CompHediff_DamageAmplifier>())
                    {
                        __result *= amplifier.Props.damageMultiplier;
                    }
                }
            }
        }
    }
}
