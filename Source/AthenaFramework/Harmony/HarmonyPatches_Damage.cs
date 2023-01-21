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
    [HarmonyPatch(typeof(DamageInfo), nameof(DamageInfo.Amount), MethodType.Getter)]
    public static class DamageInfo_AmountGetter
    {
        public static void Postfix(DamageInfo __instance, ref float __result)
        {
            if (__instance.Weapon != null && __instance.Weapon.GetModExtension<DamageModifierExtension>() != null)
            {
                __result *= __instance.Weapon.GetModExtension<DamageModifierExtension>().OutgoingDamageMultiplier;
            }

            if (__instance.Instigator == null)
            {
                return;
            }

            if (__instance.Instigator.def.GetModExtension<DamageModifierExtension>() != null)
            {
                __result *= __instance.Instigator.def.GetModExtension<DamageModifierExtension>().OutgoingDamageMultiplier;
            }

            ThingWithComps compThing = __instance.Instigator as ThingWithComps;

            if (compThing == null)
            {
                return;
            }

            for (int i = compThing.AllComps.Count - 1; i >= 0; i--)
            {
                Comp_DamageModifier modifier = compThing.AllComps[i] as Comp_DamageModifier;

                if (modifier != null)
                {
                    __result *= modifier.OutgoingDamageMultiplier;
                }
            }

            Pawn pawn = __instance.Instigator as Pawn;

            if (pawn == null)
            {
                return;
            }

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];

                if (hediff.def.GetModExtension<DamageModifierExtension>() != null)
                {
                    __result *= hediff.def.GetModExtension<DamageModifierExtension>().OutgoingDamageMultiplier;
                }

                HediffWithComps compHediff = hediff as HediffWithComps;

                if (compHediff == null)
                {
                    continue;
                }

                for (int j = compHediff.comps.Count - 1; j >= 0; j--)
                {
                    HediffComp_DamageModifier modifier = compHediff.comps[j] as HediffComp_DamageModifier;

                    if (modifier != null)
                    {
                        __result *= modifier.OutgoingDamageMultiplier;
                    }
                }

            }

            if (pawn.apparel == null)
            {
                return;
            }

            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                Apparel apparel = wornApparel[i];

                for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                {
                    Comp_DamageModifier modifier = apparel.AllComps[j] as Comp_DamageModifier;

                    if (modifier == null)
                    {
                        continue;
                    }

                    __result *= modifier.OutgoingDamageMultiplier;
                }

                if (apparel.def.GetModExtension<DamageModifierExtension>() != null)
                {
                    __result *= apparel.def.GetModExtension<DamageModifierExtension>().OutgoingDamageMultiplier;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.PreApplyDamage))]
    public static class Thing_PrePreApplyDamage
    {
        static void Prefix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed)
            {
                return;
            }

            float modifier = 1f;
            float offset = 0f;
            List<string> excludedGlobal = new List<string>();

            if (dinfo.Weapon != null && dinfo.Weapon.GetModExtension<DamageModifierExtension>() != null)
            {
                (float, float) result = dinfo.Weapon.GetModExtension<DamageModifierExtension>().GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            if (dinfo.Instigator != null)
            {
                if (dinfo.Instigator.def.GetModExtension<DamageModifierExtension>() != null)
                {
                    (float, float) result = dinfo.Instigator.def.GetModExtension<DamageModifierExtension>().GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                    modifier *= result.Item1;
                    offset += result.Item2;
                }

                ThingWithComps compThing = dinfo.Instigator as ThingWithComps;

                if (compThing != null)
                {
                    for (int i = compThing.AllComps.Count - 1; i >= 0; i--)
                    {
                        Comp_DamageModifier modifierComp = compThing.AllComps[i] as Comp_DamageModifier;

                        if (modifierComp == null)
                        {
                            continue;
                        }

                        (float, float) result = modifierComp.GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }
                }

                Pawn pawn = dinfo.Instigator as Pawn;

                if (pawn != null)
                {
                    for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 1; i--)
                    {
                        Hediff hediff = pawn.health.hediffSet.hediffs[i];

                        if (hediff.def.GetModExtension<DamageModifierExtension>() != null)
                        {
                            (float, float) result = hediff.def.GetModExtension<DamageModifierExtension>().GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                            modifier *= result.Item1;
                            offset += result.Item2;
                        }

                        HediffWithComps compHediff = hediff as HediffWithComps;

                        if (compHediff == null)
                        {
                            continue;

                        }

                        for (int j = compHediff.comps.Count - 1; j >= 0; j--)
                        {
                            HediffComp_DamageModifier modifierComp = compHediff.comps[j] as HediffComp_DamageModifier;

                            if (modifierComp == null)
                            {
                                continue;
                            }

                            (float, float) result = modifierComp.GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                            modifier *= result.Item1;
                            offset += result.Item2;
                        }
                    }

                    if (pawn.apparel != null)
                    {
                        List<Apparel> wornApparel = pawn.apparel.WornApparel;
                        for (int i = wornApparel.Count - 1; i >= 0; i--)
                        {
                            Apparel apparel = wornApparel[i];

                            for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                            {
                                Comp_DamageModifier modifierComp = apparel.AllComps[j] as Comp_DamageModifier;

                                if (modifierComp == null)
                                {
                                    continue;
                                }

                                (float, float) result = modifierComp.GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                                modifier *= result.Item1;
                                offset += result.Item2;
                            }

                            if (apparel.def.GetModExtension<DamageModifierExtension>() != null)
                            {
                                (float, float) result = apparel.def.GetModExtension<DamageModifierExtension>().GetOutcomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                                modifier *= result.Item1;
                                offset += result.Item2;
                            }
                        }
                    }
                }
            }

            if (__instance.def.GetModExtension<DamageModifierExtension>() != null)
            {
                (float, float) result = __instance.def.GetModExtension<DamageModifierExtension>().GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            ThingWithComps compThing2 = __instance as ThingWithComps;

            if (compThing2 != null)
            {
                for (int i = compThing2.AllComps.Count - 1; i >= 0; i--)
                {
                    Comp_DamageModifier modifierComp = compThing2.AllComps[i] as Comp_DamageModifier;

                    if (modifierComp == null)
                    {
                        continue;
                    }

                    (float, float) result = modifierComp.GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                    modifier *= result.Item1;
                    offset += result.Item2;
                }
            }

            Pawn pawn2 = __instance as Pawn;

            if (pawn2 != null)
            {
                for (int i = pawn2.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    Hediff hediff = pawn2.health.hediffSet.hediffs[i];

                    if (hediff.def.GetModExtension<DamageModifierExtension>() != null)
                    {
                        (float, float) result = hediff.def.GetModExtension<DamageModifierExtension>().GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }

                    HediffWithComps compHediff = hediff as HediffWithComps;

                    if (compHediff == null)
                    {
                        continue;

                    }

                    for (int j = compHediff.comps.Count - 1; j >= 0; j--)
                    {
                        HediffComp_DamageModifier modifierComp = compHediff.comps[j] as HediffComp_DamageModifier;

                        if (modifierComp != null)
                        {
                            (float, float) result = modifierComp.GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                            modifier *= result.Item1;
                            offset += result.Item2;
                        }
                    }
                }

                if (pawn2.apparel != null)
                {
                    List<Apparel> wornApparel = pawn2.apparel.WornApparel;
                    for (int i = wornApparel.Count - 1; i >= 0; i--)
                    {
                        Apparel apparel = wornApparel[i];

                        for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                        {
                            Comp_DamageModifier modifierComp = apparel.AllComps[j] as Comp_DamageModifier;

                            if (modifierComp == null)
                            {
                                continue;
                            }

                            (float, float) result = modifierComp.GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                            modifier *= result.Item1;
                            offset += result.Item2;
                        }

                        if (apparel.def.GetModExtension<DamageModifierExtension>() != null)
                        {
                            (float, float) result = apparel.def.GetModExtension<DamageModifierExtension>().GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                            modifier *= result.Item1;
                            offset += result.Item2;
                        }
                    }
                }
            }

            dinfo.SetAmount(dinfo.Amount * modifier + offset);
        }

        static void Postfix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
        {
            if (absorbed)
            {
                return;
            }

            ThingWithComps thing = __instance as ThingWithComps;

            if (thing == null)
            {
                return;
            }

            Pawn pawn = __instance as Pawn;

            if (pawn == null)
            {
                for (int i = thing.AllComps.Count - 1; i >= 0; i--)
                {
                    IDamageResponse responder = thing.AllComps[i] as IDamageResponse;

                    if (responder == null)
                    {
                        continue;
                    }

                    responder.PreApplyDamage(ref dinfo, ref absorbed);

                    if (absorbed)
                    {
                        return;
                    }
                }

                return;
            }

            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];

                    for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                    {
                        IDamageResponse responder = apparel.AllComps[j] as IDamageResponse;

                        if (responder == null)
                        {
                            continue;
                        }

                        responder.PreApplyDamage(ref dinfo, ref absorbed);

                        if (absorbed)
                        {
                            return;
                        }
                    }
                }
            }

            for (int i = pawn.AllComps.Count - 1; i >= 0; i--)
            {
                IDamageResponse responder = pawn.AllComps[i] as IDamageResponse;

                if (responder == null)
                {
                    continue;
                }

                responder.PreApplyDamage(ref dinfo, ref absorbed);

                if (absorbed)
                {
                    return;
                }
            }

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps compHediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                if (compHediff == null)
                {
                    continue;

                }

                for (int j = compHediff.comps.Count - 1; j >= 0; j--)
                {
                    IDamageResponse responder = compHediff.comps[j] as IDamageResponse;

                    if (responder == null)
                    {
                        return;
                    }

                    responder.PreApplyDamage(ref dinfo, ref absorbed);

                    if (absorbed)
                    {
                        return;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.PostApplyDamage))]
    public static class ThingWithComps_PostPostApplyDamage
    {
        static void Postfix(ThingWithComps __instance, ref DamageInfo dinfo, ref float totalDamageDealt)
        {
            Pawn pawn = __instance as Pawn;

            if (pawn == null)
            {
                for (int i = __instance.AllComps.Count - 1; i >= 0; i--)
                {
                    IDamageResponse responder = __instance.AllComps[i] as IDamageResponse;

                    if (responder == null)
                    {
                        continue;
                    }

                    responder.PostApplyDamage(ref dinfo, ref totalDamageDealt);
                }

                return;
            }

            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];

                    for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                    {
                        IDamageResponse responder = apparel.AllComps[j] as IDamageResponse;

                        if (responder == null)
                        {
                            continue;
                        }

                        responder.PostApplyDamage(ref dinfo, ref totalDamageDealt);
                    }
                }
            }

            for (int i = pawn.AllComps.Count - 1; i >= 0; i--)
            {
                IDamageResponse responder = pawn.AllComps[i] as IDamageResponse;

                if (responder == null)
                {
                    continue;
                }

                responder.PostApplyDamage(ref dinfo, ref totalDamageDealt);
            }

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps compHediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                if (compHediff == null)
                {
                    continue;

                }

                for (int j = compHediff.comps.Count - 1; j >= 0; j--)
                {
                    IDamageResponse responder = compHediff.comps[j] as IDamageResponse;

                    if (responder == null)
                    {
                        return;
                    }

                    responder.PostApplyDamage(ref dinfo, ref totalDamageDealt);
                }
            }
        }
    }

    [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
    public static class ArmorUtility_PostArmorGetter
    {
        public static void Postfix(Pawn pawn, float amount, float armorPenetration, BodyPartRecord part, ref DamageDef damageDef, ref bool deflectedByMetalArmor, ref bool diminishedByMetalArmor, ref float __result)
        {
            if (__result == 0)
            {
                return;
            }

            StatDef armorRatingStat = damageDef.armorCategory.armorRatingStat;

            if (pawn.apparel != null)
            {
                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];
                    if (apparel.def.apparel.CoversBodyPart(part))
                    {
                        for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                        {
                            IArmored armor = apparel.AllComps[j] as IArmored;

                            if (armor == null)
                            {
                                continue;
                            }

                            float num = __result;
                            bool flag;

                            armor.ApplyArmor(ref __result, armorPenetration, armorRatingStat, part, ref damageDef, out flag);

                            if (__result < 0.001f)
                            {
                                deflectedByMetalArmor = flag;
                                __result = 0f;
                                return;
                            }

                            if (__result < num && flag)
                            {
                                diminishedByMetalArmor = true;
                            }
                        }
                    }
                }
            }

            for (int i = pawn.AllComps.Count - 1; i >= 0; i--)
            {
                IArmored armor = pawn.AllComps[i] as IArmored;

                if (armor == null)
                {
                    continue;
                }

                float num3 = __result;
                bool flag3;

                armor.ApplyArmor(ref __result, armorPenetration, armorRatingStat, part, ref damageDef, out flag3);

                if (__result < 0.001f)
                {
                    deflectedByMetalArmor = flag3;
                    __result = 0f;
                    return;
                }

                if (__result < num3 && flag3)
                {
                    diminishedByMetalArmor = true;
                }
            }

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps hediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                if (hediff == null)
                {
                    continue;
                }

                for (int j = hediff.comps.Count - 1; j >= 0; j--)
                {
                    IArmored armor = hediff.comps[j] as IArmored;

                    if (armor == null)
                    {
                        continue;
                    }

                    float num = __result;
                    bool flag;

                    armor.ApplyArmor(ref __result, armorPenetration, armorRatingStat, part, ref damageDef, out flag);
                    if (__result < 0.001f)
                    {
                        deflectedByMetalArmor = flag;
                        __result = 0f;
                        return;
                    }

                    if (__result < num && flag)
                    {
                        diminishedByMetalArmor = true;
                    }
                }
            }

            __result = amount;
        }
    }
}
