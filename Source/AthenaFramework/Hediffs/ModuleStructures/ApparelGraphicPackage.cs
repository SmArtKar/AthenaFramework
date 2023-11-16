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
    public class ApparelGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;
        // When set to true, graphic will change with the owner's bodytype, similarly to apparel
        public bool useBodytype = false;

        // Coloring for the first and second masks respectively
        // In case chosen shader does not support masks, first mask acts as color
        public ApparelPackageColor firstMask = ApparelPackageColor.None;
        public ApparelPackageColor secondMask = ApparelPackageColor.None;
        
        // Gradient points. Requires firstMask to be set to either SeverityGradient or HealthGradient
        public List<GradientPoint> gradient;
        // Determines the amount of graphics generated for the gradient. Higher numbers increase RAM usage but provide a smoother transition.
        public int gradientVariants = 10;

        // If this graphic should be rendered only when the pawn is drafted. Overrides the props field
        public bool onlyRenderWhenDrafted = false;

        // List of thing styles that this package is valid for. Can be used to make separate overlays for different styles
        public List<ThingStyleDef> thingStyles;

        private List<Color> gradientList;
        private List<Graphic> gradientGraphics;
        private Graphic cachedGraphic;

        private Color cachedFirstColor;
        private Color cachedSecondColor;
        private string cachedTexturePath;

        public bool UsesGradient => firstMask == ApparelPackageColor.HealthGradient || firstMask == ApparelPackageColor.ParentGradient;

        public virtual bool CanRender(Apparel apparel, BodyTypeDef bodyType, Pawn pawn)
        {
            if (thingStyles != null && !thingStyles.Contains(apparel.StyleDef))
            {
                return false;
            }

            if (onlyRenderWhenDrafted && (pawn.drafter == null || !pawn.drafter.Drafted))
            {
                return false;
            }

            return true;
        }

        public virtual Graphic GetGraphic(Apparel apparel, BodyTypeDef bodyType = null, Color? customColor = null, float? customGradientValue = null)
        {
            if (secondMask == ApparelPackageColor.HealthGradient || secondMask == ApparelPackageColor.ParentGradient)
            {
                Log.Error("ApparelGraphicPackage secondMask set to gradient. Only firstMask supports gradients");
                return graphicData.Graphic;
            }

            Color firstColor = GetColor(firstMask, apparel, customColor, customGradientValue) ?? graphicData.color;
            Color secondColor = GetColor(secondMask, apparel, customColor, customGradientValue) ?? graphicData.colorTwo;

            string texturePath = graphicData.texPath;

            if (useBodytype && bodyType != null)
            {
                texturePath += "_" + bodyType.defName;
            }

            if (firstColor == cachedFirstColor && secondColor == cachedSecondColor && texturePath == cachedTexturePath && cachedGraphic != null)
            {
                return cachedGraphic;
            }

            cachedFirstColor = firstColor;

            if (!UsesGradient)
            {
                cachedSecondColor = secondColor;
                cachedTexturePath = texturePath;

                cachedGraphic = GraphicDatabase.Get(graphicData.graphicClass, texturePath, graphicData.Graphic.Shader, graphicData.drawSize, firstColor, secondColor);
                return cachedGraphic;
            }

            if (secondColor != cachedSecondColor || texturePath != cachedTexturePath)
            {
                cachedSecondColor = secondColor;
                cachedTexturePath = texturePath;

                CreateGradient(secondColor, texturePath);
            }

            int gradientID = 0;
            
            if (firstMask == ApparelPackageColor.HealthGradient)
            {
                gradientID = Math.Min((int)Math.Floor(apparel.Wearer.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants - 1);
            }
            else if (firstMask == ApparelPackageColor.ParentGradient)
            {
                gradientID = Math.Min((int)Math.Floor(customGradientValue.Value * gradientVariants), gradientVariants - 1);
            }

            cachedGraphic = gradientGraphics[gradientID];

            return cachedGraphic;
        }

        public virtual Color? GetColor(ApparelPackageColor type, Apparel apparel, Color? customColor, float? customGradientValue)
        {
            switch (type)
            {
                case ApparelPackageColor.ApparelColor:
                    return apparel.DrawColor;

                case ApparelPackageColor.FactionColor:
                    if (apparel.Wearer == null || apparel.Wearer.Faction == null)
                    {
                        return null;
                    }

                    return apparel.Wearer.Faction.Color;

                case ApparelPackageColor.IdeoColor:
                    if (!ModLister.IdeologyInstalled || apparel.Wearer == null || apparel.Wearer.ideo == null)
                    {
                        return null;
                    }

                    return apparel.Wearer.ideo.Ideo.Color;

                case ApparelPackageColor.FavoriteColor:
                    if (!ModLister.IdeologyInstalled || apparel.Wearer == null || apparel.Wearer.story == null)
                    {
                        return null;
                    }

                    return apparel.Wearer.story.favoriteColor;

                case ApparelPackageColor.PrimaryColor:
                    IColorSelector comp = null;

                    for (int i = apparel.comps.Count - 1; i >= 0; i--)
                    {
                        comp = apparel.comps[i] as IColorSelector;

                        if (comp != null)
                        {
                            break;
                        }
                    }

                    if (comp == null)
                    {
                        return null;
                    }

                    return comp.PrimaryColor;

                case ApparelPackageColor.SecondaryColor:
                    IColorSelector comp2 = null;

                    for (int i = apparel.comps.Count - 1; i >= 0; i--)
                    {
                        comp2 = apparel.comps[i] as IColorSelector;

                        if (comp2 != null)
                        {
                            break;
                        }
                    }

                    if (comp2 == null)
                    {
                        return null;
                    }

                    return comp2.SecondaryColor;

                case ApparelPackageColor.ParentColor:
                    return customColor;

                case ApparelPackageColor.HealthGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(apparel.Wearer.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants - 1)];

                case ApparelPackageColor.ParentGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(customGradientValue.Value * gradientVariants), gradientVariants - 1)];
            }

            return null;
        }

        public virtual List<Color> ColorGradient
        {
            get
            {
                if (gradientList == null)
                {
                    gradientList = CreateGradientPoints();
                }

                return gradientList;
            }

            set
            {
                gradientList = value;
            }
        }

        public virtual List<Color> CreateGradientPoints()
        {
            List<Color> curGradient = new List<Color>();
            GradientCurve curve = new GradientCurve(gradient);

            for (int i = 0; i < gradientVariants; i++)
            {
                float pos = i / (float)gradientVariants;
                curGradient.Add(curve.Evaluate(pos));
            }

            return curGradient;
        }

        public virtual void CreateGradient(Color secondColor, string texturePath)
        {
            gradientGraphics = new List<Graphic>();

            for (int i = 0; i < gradientVariants; i++)
            {
                gradientGraphics.Add(GraphicDatabase.Get(graphicData.graphicClass, texturePath, graphicData.Graphic.Shader, graphicData.drawSize, ColorGradient[i], secondColor));
            }
        }
    }

    public enum ApparelPackageColor
    {
        None,
        ApparelColor, //Apparel's material color
        FactionColor,
        IdeoColor,
        FavoriteColor,
        PrimaryColor,
        ParentColor,
        SecondaryColor,
        HealthGradient,
        ParentGradient,
    }
}
