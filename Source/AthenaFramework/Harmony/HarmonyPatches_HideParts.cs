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

    [HarmonyPatch(typeof(PawnRenderNode_Body), nameof(PawnRenderNode_Body.GraphicFor))]
    public static class PawnRenderer_BodyPrefix
    {
        public static bool Prefix(PawnRenderNode_Body __instance, Pawn pawn, Graphic __result)
        {
            if (AthenaCache.bodyCache == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.HideBody)
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PawnRenderNode_Hair), nameof(PawnRenderNode_Hair.GraphicFor))]
    public static class PawnRenderNode_Hair_GraphicFor
    {
        public static bool Prefix(PawnRenderNode_Hair __instance, Pawn pawn, Graphic __result)
        {
            if (AthenaCache.bodyCache == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.HideHair)
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }
    }


    [HarmonyPatch(typeof(PawnRenderNodeWorker_Head), nameof(PawnRenderNodeWorker_Head.CanDrawNow))]
    public static class PawnGraphicSet_HeadMat_CanDrawNow
    {
        public static bool Prefix(PawnRenderNodeWorker_Head __instance, PawnRenderNode node, PawnDrawParms parms, bool __result)
        {
            if (AthenaCache.bodyCache == null)
            {
                return true;
            }

            if (!AthenaCache.bodyCache.TryGetValue(node.tree.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return true;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.HideHead)
                {
                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }
}
