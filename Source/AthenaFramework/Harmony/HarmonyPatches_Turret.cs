using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    [HarmonyPatch(typeof(CompTurretGun), nameof(CompTurretGun.TurretMat), MethodType.Getter)]
    public static class CompTurretGun_TurretMat_Fixer
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator ilg)
        {
            var code = new List<CodeInstruction>(instructions);
            LocalBuilder floatLocal = ilg.DeclareLocal(typeof(float));

            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 1; i++) // -1 since we will be checking i + 1
            {
                if (code[i].opcode == OpCodes.Ldfld && (FieldInfo)code[i].operand == AccessTools.Field(typeof(Verse.GraphicData), "texPath"))
                {
                    insertionIndex = i;
                    break;
                }
            }

            List<CodeInstruction> instructionsToInsert = new List<CodeInstruction>();
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Verse.GraphicData), "get_Graphic")));
            instructionsToInsert.Add(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Verse.Graphic), "get_MatSingle")));

            if (insertionIndex != -1)
            {
                code.RemoveAt(insertionIndex);
                code.RemoveAt(insertionIndex);
                code.InsertRange(insertionIndex, instructionsToInsert);
            }

            return code;
        }
    }

    [HarmonyPatch(typeof(CompTurretGun), nameof(CompTurretGun.CanShoot), MethodType.Getter)]
    public static class CompTurretGun_CanShootGetter
    {
        public static void Postfix(CompTurretGun __instance, ref bool __result)
        {
            if (!__instance.gun.def.HasModExtension<TurretRoofBlocked>())
            {
                return;
            }

            Pawn pawn = __instance.parent as Pawn;

            if (pawn == null)
            {
                return;
            }

            RoofDef roof = pawn.Position.GetRoof(pawn.MapHeld);

            if (roof != null)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(CompTurretGun), nameof(CompTurretGun.PostDraw))]
    public static class CompTurretGun_PrePostDraw
    {
        public static bool Prefix(CompTurretGun __instance)
        {
            if (!__instance.gun.def.HasModExtension<TurretGraphicOverride>())
            {
                return true;
            }

            CompProperties_TurretGun props = __instance.props as CompProperties_TurretGun;

            if (__instance.turretMat == null)
            {
                __instance.turretMat = props.turretDef.graphicData.Graphic.MatSingle;
            }

            TurretGraphicOverride graphicOverride = __instance.gun.def.GetModExtension<TurretGraphicOverride>();

            Rot4 rotation = __instance.parent.Rotation;
            Vector3 vector = new Vector3(0f, 0.04054054f, 0f);
            if (graphicOverride.offsets != null)
            {
                if (graphicOverride.offsets.Count == 4)
                {
                    vector += graphicOverride.offsets[rotation.AsInt];
                }
                else
                {
                    vector += graphicOverride.offsets[0];
                }
            }

            Matrix4x4 matrix4x = default(Matrix4x4);
            Vector2 drawSize = props.turretDef.graphicData.drawSize;
            matrix4x.SetTRS(__instance.parent.DrawPos + vector, __instance.curRotation.ToQuat(), new Vector3(drawSize.x, 0, drawSize.y));
            Graphics.DrawMesh(MeshPool.plane10, matrix4x, __instance.turretMat, 0);
            return false;
        }
    }
}
