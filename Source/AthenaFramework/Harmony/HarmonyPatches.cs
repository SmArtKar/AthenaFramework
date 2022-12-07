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

        [HarmonyPatch(typeof(CompTurretGun), "TurretMat", MethodType.Getter)]
        public static class CompTurretGun_TurretMat_Fixer
        {
            static void Prefix(CompTurretGun __instance)
            {
                if (__instance.turretMat != null)
                {
                    return;
                }

                __instance.turretMat = (__instance.props as CompProperties_TurretGun).turretDef.graphicData.Graphic.MatSingle;
            }
        }


        /*
        [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
        public static class PawnGraphicSet_ResolveAllGraphics_Postfix
        {
            static void Postfix(PawnGraphicSet __instance)
            {
                __instance.nakedGraphic = null;
            }
        }
        */

        // Damage patches

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
                foreach (HediffComp_DamageAmplifier amplifier in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_DamageAmplifier>())
                {
                    __result *= amplifier.damageMultiplier;
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
        public static class Pawn_PostPreApplyDamage
        {
            static void Postfix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
            {
                foreach (HediffComp_Shield shield in __instance.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_Shield>())
                {
                    shield.BlockDamage(ref dinfo, ref absorbed);

                    if (absorbed)
                    {
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied")]
        public static class ThingWithComps_PreApplyDamage
        {
            static bool Prefix(StunHandler __instance, ref DamageInfo dinfo)
            {
                if (!(__instance.parent is Pawn))
                {
                    return true;
                }

                Pawn pawn = __instance.parent as Pawn;

                foreach (HediffComp_Shield shield in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_Shield>())
                {
                    if (shield.BlockStun(ref dinfo))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        // Rendering patches

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt))]
        public static class Pawn_PostDrawAt
        {
            static void Postfix(Pawn __instance, Vector3 drawLoc)
            {
                foreach (HediffComp_Renderable renderable in __instance.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_Renderable>())
                {
                    renderable.DrawAt(drawLoc);
                }
            }
        }
    }
}
