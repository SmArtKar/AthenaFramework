﻿using HarmonyLib;
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
    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
    public static class PawnGraphicSet_Resolve
    {
        public static Pawn cachedPawn;
        public static BodyTypeDef initialBodytype;
        public static HeadTypeDef initialHeadtype;

        public static void Prefix(PawnGraphicSet __instance)
        {
            cachedPawn = null;
            initialBodytype = null;

            if (__instance.pawn == null || __instance.pawn.RaceProps == null || !__instance.pawn.RaceProps.Humanlike || __instance.pawn.apparel == null || __instance.pawn.story == null || __instance.pawn.story.bodyType == null || AthenaCache.bodyCache == null)
            {
                return;
            }

            BodyTypeDef bodyType = __instance.pawn.story.bodyType;
            HeadTypeDef headType = __instance.pawn.story.headType;

            if (!AthenaCache.bodyCache.TryGetValue(__instance.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];

                if (customBody.CustomBodytype(ref bodyType))
                {
                    cachedPawn = __instance.pawn;
                    initialBodytype = __instance.pawn.story.bodyType;
                    __instance.pawn.story.bodyType = bodyType;
                }

                if (customBody.CustomHeadtype(ref headType))
                {
                    cachedPawn = __instance.pawn;
                    initialHeadtype = __instance.pawn.story.headType;
                    __instance.pawn.story.headType = headType;
                }
            }
        }

        public static void Postfix(PawnGraphicSet __instance)
        {
            if (cachedPawn != __instance.pawn)
            {
                return;
            }

            if (initialBodytype != null)
            {
                __instance.pawn.story.bodyType = initialBodytype;
                initialBodytype = null;
            }

            if (initialHeadtype != null)
            {
                __instance.pawn.story.headType = initialHeadtype;
                initialHeadtype = null;
            }

            cachedPawn = null;
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
    
    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.DrawEquipmentAiming))]
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

    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.FurMatAt))]
    public static class PawnGraphicSet_FurMat
    {
        public static void Postfix(PawnGraphicSet __instance, Rot4 facing, bool portrait, bool cached, Material __result)
        {
            if (AthenaCache.bodyCache == null)
            {
                return;
            }

            if (!AthenaCache.bodyCache.TryGetValue(__instance.pawn.thingIDNumber, out List<IBodyModifier> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IBodyModifier customBody = mods[i];
                customBody.FurMat(facing, portrait, cached, ref __result);
            }
        }
    }
}
