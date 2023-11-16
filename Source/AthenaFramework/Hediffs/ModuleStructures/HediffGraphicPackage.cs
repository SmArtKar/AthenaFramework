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
    public class HediffGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;

        // Coloring for the first and second masks respectively. Only firstMask supports gradients
        // In case chosen shader does not support masks, first mask acts as color
        public HediffPackageColor firstMask = HediffPackageColor.None;
        public HediffPackageColor secondMask = HediffPackageColor.None;

        // Gradient points. Requires firstMask to be set to either SeverityGradient or HealthGradient
        public List<GradientPoint> gradient;
        // Determines the amount of graphics generated for the gradient. Higher numbers increase RAM usage but provide a smoother transition.
        public int gradientVariants = 10;

        // If this graphic should be rendered only when the pawn is drafted. Overrides the props field
        public bool onlyRenderWhenDrafted = false;

        // If this graphic adds owner's bodytype (if such exists) to its path, similarly to apparel
        public bool useBodytype = false;

        private List<Color> gradientList;
        private List<Graphic> gradientGraphics;
        private Graphic cachedGraphic;

        private Color cachedFirstColor;
        private Color cachedSecondColor;
        private string cachedTexturePath;

        public bool UsesGradient => firstMask == HediffPackageColor.SeverityGradient || firstMask == HediffPackageColor.HealthGradient || firstMask == HediffPackageColor.ParentGradient;

        public virtual bool CanRender(Hediff hediff, BodyTypeDef bodyType, Pawn pawn)
        {
            if (onlyRenderWhenDrafted && (pawn.drafter == null || !pawn.drafter.Drafted))
            {
                return false;
            }

            return true;
        }

        public virtual Graphic GetGraphic(HediffWithComps hediff, BodyTypeDef bodyType = null, Color? customColor = null, float? customGradientValue = null)
        {
            if (secondMask == HediffPackageColor.SeverityGradient || secondMask == HediffPackageColor.HealthGradient || secondMask == HediffPackageColor.ParentGradient)
            {
                Log.Error("HediffGraphicPackage secondMask set to a gradient. Only firstMask supports gradients");
                return graphicData.Graphic;
            }

            Color firstColor = GetColor(firstMask, hediff, customColor, customGradientValue) ?? graphicData.color;
            Color secondColor = GetColor(secondMask, hediff, customColor, customGradientValue) ?? graphicData.colorTwo;

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

            if (firstMask == HediffPackageColor.SeverityGradient)
            {
                gradientID = Math.Min((int)Math.Floor(hediff.Severity * gradientVariants), gradientVariants - 1);
            }
            else if (firstMask == HediffPackageColor.HealthGradient)
            {
                gradientID = Math.Min((int)Math.Floor(hediff.pawn.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants - 1);
            }
            else if (firstMask == HediffPackageColor.ParentGradient)
            {
                gradientID = Math.Min((int)Math.Floor(customGradientValue.Value * gradientVariants), gradientVariants - 1);
            }

            cachedGraphic = gradientGraphics[gradientID];

            return cachedGraphic;
        }

        public virtual void CreateGradient(Color secondColor, string texturePath)
        {
            gradientGraphics = new List<Graphic>();

            for (int i = 0; i < gradientVariants; i++)
            {
                gradientGraphics.Add(GraphicDatabase.Get(graphicData.graphicClass, texturePath, graphicData.Graphic.Shader, graphicData.drawSize, ColorGradient[i], secondColor));
            }
        }

        public virtual Color? GetColor(HediffPackageColor type, HediffWithComps hediff, Color? customColor, float? customGradientValue)
        {
            switch (type)
            {
                case HediffPackageColor.LabelColor:
                    return hediff.LabelColor;

                case HediffPackageColor.FactionColor:
                    if (hediff.pawn.Faction == null)
                    {
                        return null;
                    }

                    return hediff.pawn.Faction.Color;

                case HediffPackageColor.IdeoColor:
                    if (!ModLister.IdeologyInstalled || hediff.pawn.ideo == null)
                    {
                        return null;
                    }

                    return hediff.pawn.ideo.Ideo.Color;

                case HediffPackageColor.FavoriteColor:
                    if (!ModLister.IdeologyInstalled || hediff.pawn.story == null)
                    {
                        return null;
                    }

                    return hediff.pawn.story.favoriteColor;

                case HediffPackageColor.SkinColor:
                    if (hediff.pawn.story == null)
                    {
                        return null;
                    }

                    return hediff.pawn.story.SkinColor;

                case HediffPackageColor.PrimaryColor:
                    IColorSelector comp = null;

                    for (int i = hediff.comps.Count - 1; i >= 0; i--)
                    {
                        comp = hediff.comps[i] as IColorSelector;

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

                case HediffPackageColor.SecondaryColor:
                    IColorSelector comp2 = null;

                    for (int i = hediff.comps.Count - 1; i >= 0; i--)
                    {
                        comp2 = hediff.comps[i] as IColorSelector;

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

                case HediffPackageColor.ParentColor:
                    return customColor;

                case HediffPackageColor.SeverityGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(hediff.Severity * gradientVariants), gradientVariants - 1)];

                case HediffPackageColor.HealthGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(hediff.pawn.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants - 1)];

                case HediffPackageColor.ParentGradient:
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
    }
    
    public enum HediffPackageColor
    {
        None,
        LabelColor,
        FactionColor,
        IdeoColor,
        FavoriteColor,
        SkinColor,
        PrimaryColor,
        SecondaryColor,
        ParentColor,
        SeverityGradient,
        HealthGradient,
        ParentGradient,
    }
}
