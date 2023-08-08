using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawPawnFur))]
    public static class PawnRenderer_FurPrefix
    {
        public static bool Prefix(PawnRenderer __instance, Vector3 shellLoc, Rot4 facing, Quaternion quat, PawnRenderFlags flags)
        {
            if (AthenaCache.bodyCache == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(__instance.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.HideFur)
                {
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawPawnBody))]
    public static class PawnRenderer_BodyPrefix
    {
        public static bool Prefix(PawnRenderer __instance, ref Vector3 rootLoc, ref float angle, ref Rot4 facing, ref RotDrawMode bodyDrawType, ref PawnRenderFlags flags, ref Mesh bodyMesh)
        {
            if (AthenaCache.bodyCache == null || __instance.pawn.RaceProps == null || __instance.graphics == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(__instance.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (!customBody.HideBody)
                {
                    continue;
                }

                if (bodyDrawType == RotDrawMode.Dessicated && !__instance.pawn.RaceProps.Humanlike && __instance.graphics.dessicatedGraphic != null && !flags.FlagSet(PawnRenderFlags.Portrait))
                {
                    bodyMesh = null;
                }
                else if (__instance.pawn.RaceProps.Humanlike)
                {
                    bodyMesh = HumanlikeMeshPoolUtility.GetHumanlikeBodySetForPawn(__instance.pawn).MeshAt(facing);
                }
                else
                {
                    bodyMesh = __instance.graphics.nakedGraphic.MeshAt(facing);
                }

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawHeadHair))]
    public static class PawnRenderer_HairPrefix
    {
        public static bool Prefix(PawnRenderer __instance, Vector3 rootLoc, Vector3 headOffset, float angle, Rot4 bodyFacing, Rot4 headFacing, RotDrawMode bodyDrawType, PawnRenderFlags flags, bool bodyDrawn)
        {
            if (AthenaCache.bodyCache == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(__instance.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.HideHair)
                {
                    return false;
                }
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.HeadMatAt))]
    public static class PawnGraphicSet_HeadMat
    {
        public static bool Prefix(PawnGraphicSet __instance, Rot4 facing, RotDrawMode bodyCondition, bool stump, bool portrait, bool allowOverride, ref Material __result)
        {
            if (AthenaCache.bodyCache == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(__instance.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.HideHead)
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }
}
