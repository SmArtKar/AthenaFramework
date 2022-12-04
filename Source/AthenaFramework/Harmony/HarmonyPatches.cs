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
        public static class AttackDetector_PostApplyDamage
        {
            static void Postfix(DamageInfo __instance, float __result)
            {
                if (__instance.Instigator == null)
                {
                    return;
                }
            }
        }
    }
}
