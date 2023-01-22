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

        private List<Color> gradientList;
        private List<Graphic> gradientGraphics;
        private Graphic cachedGraphic;

        private Color cachedFirstColor;
        private Color cachedSecondColor;

        public virtual Graphic GetGraphic(Hediff hediff)
        {
            if (secondMask == HediffPackageColor.SeverityGradient || secondMask == HediffPackageColor.HealthGradient)
            {
                Log.Error("HediffGraphicPackage secondMask set to a gradient. Only firstMask supports gradients");
                return graphicData.Graphic;
            }

            Color secondColor = GetColor(secondMask, hediff) ?? graphicData.colorTwo; //Getting second color first for the gradient creation
            bool createdGradient = false;

            if (gradientGraphics == null && (firstMask == HediffPackageColor.SeverityGradient || firstMask == HediffPackageColor.HealthGradient))
            {
                CreateGradient(secondColor);
                createdGradient = true;
            }

            Color firstColor = GetColor(firstMask, hediff) ?? graphicData.color;

            if (firstColor == cachedFirstColor && secondColor == cachedSecondColor)
            {
                return cachedGraphic;
            }

            cachedFirstColor = firstColor;

            if (gradientGraphics == null)
            {
                cachedSecondColor = secondColor;
                cachedGraphic = graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, firstColor, secondColor);
                return cachedGraphic;
            }

            if (secondColor != cachedSecondColor)
            {
                cachedSecondColor = secondColor;

                if (!createdGradient)
                {
                    CreateGradient(secondColor);
                }
            }

            cachedGraphic = gradientGraphics[Math.Min((int)Math.Floor((firstMask == HediffPackageColor.SeverityGradient ? hediff.Severity : hediff.pawn.health.summaryHealth.SummaryHealthPercent) * gradientVariants), gradientVariants - 1)];

            return cachedGraphic;
        }

        public virtual void CreateGradient(Color secondColor)
        {
            gradientGraphics = new List<Graphic>();

            for (int i = 0; i < gradientVariants; i++)
            {
                gradientGraphics.Add(graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, ColorGradient[i], secondColor));
            }
        }

        public virtual Color? GetColor(HediffPackageColor type, Hediff hediff)
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

                case HediffPackageColor.SeverityGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(hediff.Severity * gradientVariants), gradientVariants - 1)];

                case HediffPackageColor.HealthGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(hediff.pawn.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants - 1)];

                case HediffPackageColor.PrimaryColor:
                    HediffComp_Renderable comp = hediff.TryGetComp<HediffComp_Renderable>();

                    if (comp == null)
                    {
                        return null;
                    }

                    return comp.primaryColor;

                case HediffPackageColor.SecondaryColor:
                    HediffComp_Renderable comp2 = hediff.TryGetComp<HediffComp_Renderable>();

                    if (comp2 == null)
                    {
                        return null;
                    }

                    return comp2.secondaryColor;
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
        SeverityGradient,
        HealthGradient,
        PrimaryColor,
        SecondaryColor
    }
}
