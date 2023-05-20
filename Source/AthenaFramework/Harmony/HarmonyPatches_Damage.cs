using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Mono.Cecil.Cil;
using static UnityEngine.GraphicsBuffer;

namespace AthenaFramework
{
    #region ===== Damage Modification =====

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

            if (AthenaCache.damageCache.TryGetValue(__instance.Instigator.thingIDNumber, out List<IDamageModifier> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    __result *= mods[i].OutgoingDamageMultiplier;
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
            }

            if (pawn.apparel == null)
            {
                return;
            }

            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                Apparel apparel = wornApparel[i];

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

            if (AthenaCache.damageCache.TryGetValue(__instance.thingIDNumber, out List<IDamageModifier> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    IDamageModifier modifierComp = mods[i];

                    (float, float) result = modifierComp.GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                    modifier *= result.Item1;
                    offset += result.Item2;
                }
            }

            if (__instance.def.GetModExtension<DamageModifierExtension>() != null)
            {
                (float, float) result = __instance.def.GetModExtension<DamageModifierExtension>().GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
                modifier *= result.Item1;
                offset += result.Item2;
            }

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

                if (AthenaCache.damageCache.TryGetValue(dinfo.Instigator.thingIDNumber, out List<IDamageModifier> mods2))
                {
                    for (int i = mods2.Count - 1; i >= 0; i--)
                    {
                        IDamageModifier modifierComp = mods2[i];

                        (float, float) result = modifierComp.GetIncomingDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator, dinfo);
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
                    }

                    if (pawn.apparel != null)
                    {
                        List<Apparel> wornApparel = pawn.apparel.WornApparel;
                        for (int i = wornApparel.Count - 1; i >= 0; i--)
                        {
                            Apparel apparel = wornApparel[i];

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
                }

                if (pawn2.apparel != null)
                {
                    List<Apparel> wornApparel = pawn2.apparel.WornApparel;
                    for (int i = wornApparel.Count - 1; i >= 0; i--)
                    {
                        Apparel apparel = wornApparel[i];

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

            if (!AthenaCache.responderCache.TryGetValue(__instance.thingIDNumber, out List<IDamageResponse> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IDamageResponse responder = mods[i];

                responder.PreApplyDamage(ref dinfo, ref absorbed);

                if (absorbed)
                {
                    return;
                }
            }
        }
    }

    #endregion

    #region ===== Armor Transpiler =====

    [HarmonyPatch(typeof(ArmorUtility), nameof(ArmorUtility.GetPostArmorDamage))]
    public static class ArmorUtility_ArmorTranspiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var code = new List<CodeInstruction>(instructions);
            LocalBuilder floatLocal = ilg.DeclareLocal(typeof(float));
            LocalBuilder floatLocal2 = ilg.DeclareLocal(typeof(float));
            Label ifLabel = ilg.DefineLabel();

            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldloc_S && (((code[i].operand is LocalBuilder) && ((LocalBuilder)code[i].operand).LocalIndex == 5) || ((code[i].operand is int) && Convert.ToInt32(code[i].operand) == 5)))
                {
                    insertionIndex = i;
                    break;
                }
            }

            if (insertionIndex == -1)
            {
                return code;
            }

            object operand1 = code[insertionIndex + 8].operand;
            object operand2 = code[insertionIndex + 16].operand;

            List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldc_I4_M1));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue), new Type[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(Int32) })));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Stloc_S, floatLocal));

            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarga_S, operand1));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_2));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_0));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloca_S, floatLocal));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_3));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_S, operand2));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AthenaCombatUtility), nameof(AthenaCombatUtility.CoversBodyPart), new Type[] { typeof(Thing), typeof(float).MakeByRefType(), typeof(float), typeof(StatDef), typeof(float).MakeByRefType(), typeof(BodyPartRecord), typeof(DamageDef).MakeByRefType(), typeof(Pawn) })));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Brfalse_S, ifLabel));

            List<CodeInstruction> instructionsToInsert2 = new List<CodeInstruction>();
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldloc_S, 5));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldarga_S, operand1));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldarg_2));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldloc_0));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldloc_S, floatLocal));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldarg_3));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldarg_S, operand2));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Ldloca_S, 6));
            instructionsToInsert2.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AthenaCombatUtility), nameof(AthenaCombatUtility.ApplyArmor), new Type[] { typeof(Thing), typeof(float).MakeByRefType(), typeof(float), typeof(StatDef), typeof(float), typeof(BodyPartRecord), typeof(DamageDef).MakeByRefType(), typeof(Pawn), typeof(bool).MakeByRefType() })));

            code.RemoveRange(insertionIndex + 1, 5);
            code.InsertRange(insertionIndex + 1, instructionsToInsert);

            code.RemoveRange(insertionIndex + 18, 12);
            code.InsertRange(insertionIndex + 18, instructionsToInsert2);

            int insertionIndex2 = -1;
            Byte num = 1;
            for (int i = insertionIndex + 53; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldarga_S && (Byte)code[i].operand == num)
                {
                    insertionIndex2 = i;
                    break;
                }
            }

            List<CodeInstruction> instructionsToInsert3 = new List<CodeInstruction>();

            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldloc_0));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldc_I4_M1));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue), new Type[] { typeof(Thing), typeof(StatDef), typeof(bool), typeof(Int32) })));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Stloc_S, floatLocal2));

            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldnull));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldarga_S, operand1));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldarg_2));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldloc_0));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldloc_S, floatLocal2));

            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldarg_3));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldarg_S, operand2));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Ldloca_S, 1));
            instructionsToInsert3.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AthenaCombatUtility), nameof(AthenaCombatUtility.ApplyArmor), new Type[] { typeof(Thing), typeof(float).MakeByRefType(), typeof(float), typeof(StatDef), typeof(float), typeof(BodyPartRecord), typeof(DamageDef).MakeByRefType(), typeof(Pawn), typeof(bool).MakeByRefType() })));

            code.RemoveRange(insertionIndex2, 12);
            code.InsertRange(insertionIndex2, instructionsToInsert3);

            int insertionIndex3 = -1;
            for (int i = insertionIndex; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Sub)
                {
                    insertionIndex3 = i;
                    break;
                }
            }

            code[insertionIndex3 - 2].labels.Add(ifLabel);

            return code;
        }
    }

    #endregion

    #region ===== Tool Transpiling =====

    [HarmonyPatch(typeof(Verb_MeleeAttackDamage), nameof(Verb_MeleeAttackDamage.DamageInfosToApply))]
    public static class MeleeAttack_Damage
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var code = new List<CodeInstruction>(instructions);

            object operand1 = null;
            object operand2 = null;

            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Callvirt && (MethodInfo)code[i].operand == AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedMeleeDamageAmount), new Type[] { typeof(Verb), typeof(Pawn) }))
                {
                    operand1 = code[i + 1].operand;
                }

                if (code[i].opcode == OpCodes.Callvirt && (MethodInfo)code[i].operand == AccessTools.Method(typeof(VerbProperties), nameof(VerbProperties.AdjustedArmorPenetration), new Type[] { typeof(Verb), typeof(Pawn) }))
                {
                    insertionIndex = i;
                    break;
                }
            }

            if (insertionIndex == -1)
            {
                return code;
            }

            List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();

            operand2 = code[insertionIndex + 1].operand;

            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloc_2));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloca_S, operand1));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldloca_S, operand2));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldarg_0));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(Verb_MeleeAttackDamage), "target")));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AthenaCombatUtility), nameof(AthenaCombatUtility.DamageModification), new Type[] { typeof(Verb), typeof(float).MakeByRefType(), typeof(float).MakeByRefType(), typeof(LocalTargetInfo).MakeByRefType() })));

            code.InsertRange(insertionIndex + 2, instructionsToInsert);

            return code;
        }
    }

    [HarmonyPatch(typeof(VerbProperties), nameof(VerbProperties.AdjustedCooldown), new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing) })]
    public static class VerbProperties_Cooldown
    {
        public static void Postfix(VerbProperties __instance, Tool tool, Pawn attacker, Thing equipment, ref float __result)
        {
            AdvancedTool advTool = tool as AdvancedTool;

            if (advTool != null)
            {
                advTool.CooldownModification(__instance, ref __result, tool, attacker, equipment);
            }
        }
    }

    #endregion

}
