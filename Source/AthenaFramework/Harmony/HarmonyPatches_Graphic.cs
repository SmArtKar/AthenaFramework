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
    [HarmonyPatch(typeof(PawnGraphicSet), nameof(PawnGraphicSet.ResolveAllGraphics))]
    public static class PawnGraphicSet_Resolve
    {
        public static Pawn cachedPawn;
        public static BodyTypeDef initialBodytype;

        public static void Prefix(PawnGraphicSet __instance)
        {
            cachedPawn = null;
            initialBodytype = null;

            if (!__instance.pawn.RaceProps.Humanlike || __instance.pawn.apparel == null || __instance.pawn.story == null || __instance.pawn.story.bodyType == null)
            {
                return;
            }

            BodyTypeDef bodyType = __instance.pawn.story.bodyType;

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
                    return;
                }
            }
        }

        public static void Postfix(PawnGraphicSet __instance)
        {
            if (cachedPawn != __instance.pawn || initialBodytype == null)
            {
                return;
            }

            __instance.pawn.story.bodyType = initialBodytype;
            cachedPawn = null;
            initialBodytype = null;
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

                if (customBody.PreventBodytype(bodyType))
                {
                    if (apparel.WornGraphicPath.NullOrEmpty())
                    {
                        return true;
                    }

                    Shader shader = ShaderDatabase.Cutout;
                    if (apparel.def.apparel.useWornGraphicMask)
                    {
                        shader = ShaderDatabase.CutoutComplex;
                    }

                    Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.WornGraphicPath, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
                    rec = new ApparelGraphicRecord(graphic, apparel);

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
}
