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
    public class Comp_AdditionalApparelGraphics : ThingComp
    {
        private CompProperties_AdditionalApparelGraphics Props => props as CompProperties_AdditionalApparelGraphics;
        private Apparel Apparel => parent as Apparel;
        private Pawn Pawn => Apparel.Wearer as Pawn;

        public void Draw(Vector3 drawPos)
        {
            List<Apparel> wornApparel = Pawn.apparel.WornApparel;
            BodyTypeDef bodyType = Pawn.story.bodyType;

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

                    BodyTypeDef newBodyType = customBody.CustomBodytype(Apparel, bodyType);

                    if (newBodyType != null)
                    {
                        bodyType = newBodyType;
                    }
                }
            }


        }
    }

    public class CompProperties_AdditionalApparelGraphics : CompProperties
    {
        public CompProperties_AdditionalApparelGraphics()
        {
            this.compClass = typeof(Comp_AdditionalApparelGraphics);
        }

        // Additional graphic layers with precise controls
        public List<ApparelGraphicPackage> additionalGraphics = new List<ApparelGraphicPackage>();
    }

    public enum ApparelPackageColor
    {
        None,
        ApparelColor,
        FactionColor,
        IdeoColor,
        FavoriteColor
    }

    public class ApparelGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;

        // Coloring for the first and second masks respectively. Only firstMask supports gradients
        public ApparelPackageColor firstMask = ApparelPackageColor.None;
        public ApparelPackageColor secondMask = ApparelPackageColor.None;

        private Graphic cachedGraphic;

        private Color cachedFirstColor;
        private Color cachedSecondColor;

        public Graphic GetGraphic(Apparel apparel)
        {
            if (!ShaderUtility.SupportsMaskTex(graphicData.Graphic.Shader))
            {
                return graphicData.Graphic;
            }

            Color firstColor = GetColor(firstMask, apparel);
            Color secondColor = GetColor(secondMask, apparel);

            if (firstColor == cachedFirstColor && secondColor == cachedSecondColor)
            {
                return cachedGraphic;
            }

            cachedFirstColor = firstColor;
            cachedSecondColor = secondColor;

            cachedGraphic = graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, firstColor, secondColor);
            return cachedGraphic;
        }

        public Color GetColor(ApparelPackageColor type, Apparel apparel)
        {
            switch (type)
            {
                case ApparelPackageColor.ApparelColor:
                    return apparel.DrawColor;

                case ApparelPackageColor.FactionColor:
                    if (apparel.Wearer == null || apparel.Wearer.Faction == null)
                    {
                        return Color.white;
                    }

                    return apparel.Wearer.Faction.Color;

                case ApparelPackageColor.IdeoColor:
                    if (!ModLister.IdeologyInstalled || apparel.Wearer == null || apparel.Wearer.ideo == null)
                    {
                        return Color.white;
                    }

                    return apparel.Wearer.ideo.Ideo.Color;

                case ApparelPackageColor.FavoriteColor:
                    if (!ModLister.IdeologyInstalled || apparel.Wearer == null || apparel.Wearer.story == null || apparel.Wearer.story.favoriteColor == null)
                    {
                        return Color.white;
                    }

                    return (Color)apparel.Wearer.story.favoriteColor;
            }

            return Color.white;
        }
    }
}
