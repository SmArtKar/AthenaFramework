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
    [HarmonyPatch(typeof(PawnRenderNode), nameof(PawnRenderNode.GraphicFor))]
    public static class PawnRenderNode_GraphicFor_CustomBody
    {
        public static Dictionary<Pawn, BodyTypeDef> cachedBodies = new Dictionary<Pawn, BodyTypeDef>();
        public static Dictionary<Pawn, HeadTypeDef> cachedHeads = new Dictionary<Pawn, HeadTypeDef>();

        public static void Prefix(PawnRenderNode __instance, Pawn pawn)
        {

            if (pawn == null || pawn.RaceProps == null || !pawn.RaceProps.Humanlike || pawn.apparel == null || pawn.story == null || pawn.story.bodyType == null || AthenaCache.bodyCache == null)
            {
                return;
            }

            if (!AthenaCache.bodyCache.TryGetValue(pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return;
            }

            BodyTypeDef bodyType = pawn.story.bodyType;
            HeadTypeDef headType = pawn.story.headType;

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.CustomBodytype(ref bodyType))
                {
                    cachedBodies[pawn] = pawn.story.bodyType;
                    pawn.story.bodyType = bodyType;
                }

                if (customBody.CustomHeadtype(ref headType))
                {
                    cachedHeads[pawn] = pawn.story.headType;
                    pawn.story.headType = headType;
                }
            }
        }

        public static void Postfix(PawnRenderNode __instance, Pawn pawn)
        {
            if (cachedBodies.ContainsKey(pawn))
            {
                pawn.story.bodyType = cachedBodies[pawn];
                cachedBodies.Remove(pawn);
            }

            if (cachedHeads.ContainsKey(pawn))
            {
                pawn.story.headType = cachedHeads[pawn];
                cachedHeads.Remove(pawn);
            }
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel))]
    public static class ApparelGraphic_PreGet
    {
        public static bool Prefix(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec, ref bool __result)
        {
            for (int i = apparel.AllComps.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = apparel.AllComps[i] as IBodyModifier;

                if (customBody == null)
                {
                    continue;
                }

                if (customBody.CustomApparelTexture(bodyType, apparel, ref rec))
                {
                    __result = true;
                    return false;
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt))]
    public static class Pawn_PostDrawAt
    {
        public static void Postfix(Pawn __instance, Vector3 drawLoc)
        {
            BodyTypeDef bodyType = null;

            if (__instance.story != null && __instance.story.bodyType != null)
            {
                bodyType = __instance.story.bodyType;

                if (AthenaCache.bodyCache.TryGetValue(__instance.thingIDNumber, out List<IBodyModifier> mods))
                {
                    for (int i = mods.Count - 1; i >= 0; i--)
                    {
                        IBodyModifier customBody = mods[i];

                        if (customBody.CustomBodytype(ref bodyType))
                        {
                            break;
                        }
                    }
                }
            }

            if (AthenaCache.renderCache.TryGetValue(__instance.thingIDNumber, out List<IRenderable> mods2))
            {
                for (int i = mods2.Count - 1; i >= 0; i--)
                {

                    IRenderable renderable = mods2[i];
                    renderable.DrawAt(drawLoc, bodyType);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAiming))]
    public static class PawnRenderer_DrawEquipmentAiming_Offset
    {
        public static void Prefix(PawnRenderer __instance, Thing eq, ref Vector3 drawLoc, ref float aimAngle)
        {
            ThingWithComps thing = eq as ThingWithComps;

            if (thing == null)
            {
                return;
            }

            AimAngleOffsetExtension ext = thing.def.GetModExtension<AimAngleOffsetExtension>();

            if (ext != null)
            {
                aimAngle += ext.angleOffset;
            }
        }
    }

    [HarmonyPatch(typeof(PawnRenderNode_Fur), nameof(PawnRenderNode_Fur.GraphicFor))]
    public static class PawnRenderNode_Fur_GraphicFor
    {
        public static bool Prefix(PawnRenderNode_Fur __instance, Pawn pawn, Graphic __result)
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

                if (customBody.HideFur)
                {
                    __result = null;
                    return false;
                }
            }

            return true;
        }

        public static void Postfix(PawnRenderNode_Fur __instance, Pawn pawn, Graphic __result)
        {
            if (AthenaCache.bodyCache == null)
            {
                return;
            }

            if (!AthenaCache.bodyCache.TryGetValue(pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];
                customBody.FurGraphic(ref __result);
            }
        }
    }
}
