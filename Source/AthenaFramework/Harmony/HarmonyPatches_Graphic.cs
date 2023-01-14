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
    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
    public static class ApparelGraphic_PreGet
    {
        public static bool Prefix(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec, ref bool __result)
        {
            for (int i = apparel.AllComps.Count - 1; i >= 0; i--)
            {
                Comp_CustomApparelBody customBody = apparel.AllComps[i] as Comp_CustomApparelBody;

                if (customBody == null)
                {
                    continue;
                }

                if (customBody.PreventBodytype(bodyType, rec))
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

            if (apparel.Wearer == null || apparel.Wearer.apparel == null) //Somehow, this happened, be that another mod's intervention or something else.
            {
                return true;
            }

            List<Apparel> wornApparel = apparel.Wearer.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                Apparel otherApparel = wornApparel[i];

                for (int j = otherApparel.AllComps.Count - 1; j >= 0; j--)
                {
                    Comp_CustomApparelBody customBody = otherApparel.AllComps[j] as Comp_CustomApparelBody;

                    if (customBody == null)
                    {
                        continue;
                    }

                    BodyTypeDef newBodyType = customBody.CustomBodytype(apparel, bodyType, rec);

                    if (newBodyType != null)
                    {
                        bodyType = newBodyType;
                        return true;
                    }
                }
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    public static class PawnGraphicSet_PostResolve
    {
        public static void Postfix(PawnGraphicSet __instance)
        {
            if (!__instance.pawn.RaceProps.Humanlike || __instance.pawn.apparel == null)
            {
                return;
            }

            bool graphicsSet = false;
            BodyTypeDef customUserBody = null;

            List<Apparel> wornApparel = __instance.pawn.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                Apparel apparel = wornApparel[i];

                for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                {
                    Comp_CustomApparelBody customBody = apparel.AllComps[j] as Comp_CustomApparelBody;

                    if (customBody == null)
                    {
                        continue;
                    }

                    Graphic customBodyGraphic = customBody.GetBodyGraphic;

                    if (customBodyGraphic != null)
                    {
                        __instance.nakedGraphic = customBodyGraphic;
                        graphicsSet = true;
                    }

                    Graphic customHeadGraphic = customBody.GetHeadGraphic;

                    if (customHeadGraphic != null)
                    {
                        __instance.headGraphic = customHeadGraphic;
                        graphicsSet = true;
                    }

                    if (customUserBody == null)
                    {
                        BodyTypeDef customBodytype = customBody.CustomBodytype(apparel, __instance.pawn.story.bodyType);

                        if (customBodytype != null)
                        {
                            customUserBody = customBodytype;
                        }
                    }
                }
            }

            if (graphicsSet)
            {
                __instance.CalculateHairMats();
                __instance.ResolveApparelGraphics();
                __instance.ResolveGeneGraphics();
            }
            else if (customUserBody != null && __instance.pawn != null)
            {
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    Color color = (__instance.pawn.story.SkinColorOverriden ? (PawnGraphicSet.RottingColorDefault * __instance.pawn.story.SkinColor) : PawnGraphicSet.RottingColorDefault);
                    __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(customUserBody.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, __instance.pawn.story.SkinColor);
                    __instance.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(customUserBody.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, color);
                    __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(customUserBody.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);

                    __instance.CalculateHairMats();
                    __instance.ResolveApparelGraphics();
                    __instance.ResolveGeneGraphics();
                });
            }
        }
    }

    [HarmonyPatch(typeof(Pawn), "DrawAt")]
    public static class Pawn_PostDrawAt
    {
        public static void Postfix(Pawn __instance, Vector3 drawLoc)
        {
            for (int i = __instance.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps hediff = __instance.health.hediffSet.hediffs[i] as HediffWithComps;

                if (hediff == null)
                {
                    continue;
                }

                for (int j = hediff.comps.Count - 1; j >= 0; j--)
                {
                    HediffComp_Renderable renderable = hediff.comps[j] as HediffComp_Renderable;

                    if (renderable != null)
                    {
                        renderable.DrawAt(drawLoc);
                    }
                }
            }

            if (__instance.apparel != null)
            {
                List<Apparel> wornApparel = __instance.apparel.WornApparel;

                if (__instance.story == null || __instance.story.bodyType == null)
                {
                    for (int i = wornApparel.Count - 1; i >= 0; i--)
                    {
                        Apparel apparel = wornApparel[i];

                        for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                        {
                            Comp_AdditionalApparelGraphics additionalGraphics = apparel.comps[j] as Comp_AdditionalApparelGraphics;

                            if (additionalGraphics != null)
                            {
                                additionalGraphics.DrawAt(drawLoc, null);
                            }
                        }
                    }

                    return;
                }

                BodyTypeDef bodyType = __instance.story.bodyType;

                List<Comp_AdditionalApparelGraphics> compCache = new List<Comp_AdditionalApparelGraphics>();

                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];

                    for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                    {
                        Comp_CustomApparelBody customBody = apparel.AllComps[j] as Comp_CustomApparelBody;

                        if (customBody != null)
                        {
                            BodyTypeDef customBodytype = customBody.CustomBodytype(apparel, __instance.story.bodyType);

                            if (customBodytype != null)
                            {
                                bodyType = customBodytype;
                            }
                        }

                        Comp_AdditionalApparelGraphics additionalGraphics = apparel.comps[j] as Comp_AdditionalApparelGraphics;

                        if (additionalGraphics != null)
                        {
                            compCache.Add(additionalGraphics);
                        }
                    }
                }

                for (int i = compCache.Count - 1; i >= 0; i--)
                {
                    Comp_AdditionalApparelGraphics additionalGraphics = compCache[i];
                    additionalGraphics.DrawAt(drawLoc, bodyType);
                }
            }
        }
    }
    
    [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
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
