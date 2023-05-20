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
    public class DroneGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;

        // Coloring for the first and second masks respectively. Only firstMask supports gradients
        // In case chosen shader does not support masks, first mask acts as color
        public DronePackageColor firstMask = DronePackageColor.None;
        public DronePackageColor secondMask = DronePackageColor.None;

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

        public virtual Graphic GetGraphic(Drone drone)
        {
            if (secondMask == DronePackageColor.PawnHealthGradient || secondMask == DronePackageColor.DroneHealthGradient)
            {
                Log.Error("DroneGraphicPackage secondMask set to a gradient. Only firstMask supports gradients");
                return graphicData.Graphic;
            }

            Color secondColor = GetColor(secondMask, drone) ?? graphicData.colorTwo; //Getting second color first for the gradient creation
            bool createdGradient = false;

            if (gradientGraphics == null && (firstMask == DronePackageColor.PawnHealthGradient || firstMask == DronePackageColor.DroneHealthGradient))
            {
                CreateGradient(secondColor);
                createdGradient = true;
            }

            Color firstColor = GetColor(firstMask, drone) ?? graphicData.color;

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

            cachedGraphic = gradientGraphics[Math.Min((int)Math.Floor((firstMask == DronePackageColor.DroneHealthGradient ? drone.health / drone.MaxHealth : drone.pawn.health.summaryHealth.SummaryHealthPercent) * gradientVariants), gradientVariants - 1)];

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

        public virtual Color? GetColor(DronePackageColor type, Drone drone)
        {
            switch (type)
            {
                case DronePackageColor.FactionColor:
                    if (drone.pawn.Faction == null)
                    {
                        return null;
                    }

                    return drone.pawn.Faction.Color;

                case DronePackageColor.IdeoColor:
                    if (!ModLister.IdeologyInstalled || drone.pawn.ideo == null)
                    {
                        return null;
                    }

                    return drone.pawn.ideo.Ideo.Color;

                case DronePackageColor.FavoriteColor:
                    if (!ModLister.IdeologyInstalled || drone.pawn.story == null)
                    {
                        return null;
                    }

                    return drone.pawn.story.favoriteColor;

                case DronePackageColor.DroneHealthGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(drone.health / drone.MaxHealth * gradientVariants), gradientVariants - 1)];

                case DronePackageColor.PawnHealthGradient:
                    return ColorGradient[Math.Min((int)Math.Floor(drone.pawn.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants - 1)];

                case DronePackageColor.PrimaryColor:
                    return drone.PrimaryColor;

                case DronePackageColor.SecondaryColor:
                    return drone.SecondaryColor;
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

    public enum DronePackageColor
    {
        None,
        FactionColor,
        IdeoColor,
        FavoriteColor,
        PawnHealthGradient,
        DroneHealthGradient,
        PrimaryColor,
        SecondaryColor
    }
}
