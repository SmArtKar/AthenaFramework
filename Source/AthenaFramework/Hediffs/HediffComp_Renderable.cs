using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
using System.Reflection;

namespace AthenaFramework
{
    public class HediffComp_Renderable : HediffComp
    {
        private HediffCompProperties_Renderable Props => props as HediffCompProperties_Renderable;
        private static readonly float altitude = AltitudeLayer.MoteOverhead.AltitudeFor();

        public Mote attachedMote;

        public virtual void DrawAt(Vector3 drawPos)
        {
            if (Props.graphicData == null)
            {
                return;
            }

            Props.graphicData.Graphic.Draw(new Vector3(drawPos.x, altitude, drawPos.z), Pawn.Rotation, Pawn);

            for (int i = Props.additionalGraphics.Count - 1; i>= 0; i--)
            {
                HediffGraphicPackage package = Props.additionalGraphics[i];
                Vector3 offset = new Vector3();

                if (package.offsets!= null)
                {
                    if (package.offsets.Count == 4)
                    {
                        offset = package.offsets[Pawn.Rotation.AsInt];
                    }
                    else
                    {
                        offset = package.offsets[0];
                    }
                }

                package.GetGraphic(parent).Draw(drawPos + offset, Pawn.Rotation, Pawn);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            
            if (attachedMote != null && !attachedMote.Destroyed)
            {
                attachedMote.Maintain();
            }

            else if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (attachedMote != null && !attachedMote.Destroyed)
            {
                attachedMote.Destroy();
            }
        }
    }

    public class HediffCompProperties_Renderable : HediffCompProperties
    {
        public HediffCompProperties_Renderable()
        {
            this.compClass = typeof(HediffComp_Renderable);
        }

        // Displayed graphic. This graphic is always drawn above the pawn at MoteOverhead altitude layer
        public GraphicData graphicData;
        // Mote attached to the hediff
        public ThingDef attachedMoteDef;
        // Offset of the attached mote
        public Vector3 attachedMoteOffset = new Vector3();
        // Scale of the attached mote
        public float attachedMoteScale = 1f;
        // If set to true, attached mote will be destroyed after hediff's removal
        public bool destroyMoteOnRemoval = true;
        // Additional graphic layers with more precise controls. These are drawn on the same layer as pawn by default
        public List<HediffGraphicPackage> additionalGraphics = new List<HediffGraphicPackage>();
    }

    public enum HediffPackageColor
    {
        None,
        LabelColor,
        FactionColor,
        IdeoColor,
        SkinColor,
        FavoriteColor,
        SeverityGradient,
        HealthGradient
    }

    public class HediffGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;

        // Coloring for the first and second masks respectively. Only firstMask supports gradients
        public HediffPackageColor firstMask = HediffPackageColor.None;
        public HediffPackageColor secondMask = HediffPackageColor.None;

        // Gradient points. Requires firstMask to be set to either SeverityGradient or HealthGradient
        public List<GradientPoint> gradient;
        // Determines the amount of graphics generated for the gradient. Higher numbers increase RAM usage but provide a smoother transition.
        public int gradientVariants = 10;

        private List<Color> gradientList;
        private List<Graphic> gradientGraphics;
        private Graphic cachedGraphic;

        private Color cachedFirstColor;
        private Color cachedSecondColor;

        public Graphic GetGraphic(Hediff hediff)
        {
            if (!ShaderUtility.SupportsMaskTex(graphicData.Graphic.Shader))
            {
                return graphicData.Graphic;
            }

            if (secondMask == HediffPackageColor.SeverityGradient || secondMask == HediffPackageColor.HealthGradient)
            {
                Log.Error("HediffGraphicPackage secondMask set to a gradient. Only firstMask supports gradients");
                return graphicData.Graphic;
            }

            Color secondColor = GetColor(secondMask, hediff); //Getting second color first for the gradient creation

            if (gradientGraphics == null && (firstMask == HediffPackageColor.SeverityGradient || firstMask == HediffPackageColor.HealthGradient))
            {
                CreateGradient(secondColor);
            }

            Color firstColor = GetColor(firstMask, hediff);

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
                CreateGradient(secondColor);
            }

            cachedGraphic = gradientGraphics[Math.Min((int)Math.Floor((firstMask == HediffPackageColor.SeverityGradient ? hediff.Severity : hediff.pawn.health.summaryHealth.SummaryHealthPercent) * gradientVariants), gradientVariants)];
            return cachedGraphic;
        }

        public void CreateGradient(Color secondColor)
        {
            gradientGraphics = new List<Graphic>();

            for (int i = 0; i < gradientVariants; i++)
            {
                gradientGraphics.Add(graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, gradientList[i], secondColor));
            }
        }

        public Color GetColor(HediffPackageColor type, Hediff hediff)
        {
            switch (type)
            {
                case HediffPackageColor.LabelColor:
                    return hediff.LabelColor;

                case HediffPackageColor.FactionColor:
                    if (hediff.pawn.Faction == null)
                    {
                        return Color.white;
                    }

                    return hediff.pawn.Faction.Color;

                case HediffPackageColor.IdeoColor:
                    if (!ModLister.IdeologyInstalled || hediff.pawn.ideo == null)
                    {
                        return Color.white;
                    }

                    return hediff.pawn.ideo.Ideo.Color;

                case HediffPackageColor.SkinColor:
                    if (hediff.pawn.story == null)
                    {
                        return Color.white;
                    }

                    return hediff.pawn.story.SkinColor;

                case HediffPackageColor.FavoriteColor:
                    if (!ModLister.IdeologyInstalled || hediff.pawn.story == null || hediff.pawn.story.favoriteColor == null)
                    {
                        return Color.white;
                    }

                    return (Color)hediff.pawn.story.favoriteColor;

                case HediffPackageColor.SeverityGradient:
                    return gradientList[Math.Min((int)Math.Floor(hediff.Severity * gradientVariants), gradientVariants)];

                case HediffPackageColor.HealthGradient:
                    return gradientList[Math.Min((int)Math.Floor(hediff.pawn.health.summaryHealth.SummaryHealthPercent * gradientVariants), gradientVariants)];
            }

            return Color.white;
        }

        public List<Color> ColorGradient
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

        public List<Color> CreateGradientPoints()
        {
            List<Color> curGradient = new List<Color>();
            GradientCurve curve = new GradientCurve(gradient);

            for (int i = 0; i < gradientVariants; i++)
            {
                curGradient.Add(curve.Evaluate(1 / gradientVariants * i));
            }

            return curGradient;
        }
    }

    public class GradientCurve
    {
        public List<GradientPoint> points;
        public List<SimpleCurve> curves;

        public GradientCurve(List<GradientPoint> points)
        {
            this.points = points;
            GenerateCurves();
        }

        public void GenerateCurves()
        {
            List<CurvePoint> pointsR = new List<CurvePoint>();
            List<CurvePoint> pointsG = new List<CurvePoint>();
            List<CurvePoint> pointsB = new List<CurvePoint>();
            List<CurvePoint> pointsA = new List<CurvePoint>();

            for (int i = 0; i < points.Count; i++)
            {
                GradientPoint point = points[i];
                pointsR.Add(new CurvePoint(point.position, point.color.r));
                pointsG.Add(new CurvePoint(point.position, point.color.g));
                pointsB.Add(new CurvePoint(point.position, point.color.b));
                pointsA.Add(new CurvePoint(point.position, point.color.a));
            }

            curves.Add(new SimpleCurve(pointsR));
            curves.Add(new SimpleCurve(pointsG));
            curves.Add(new SimpleCurve(pointsB));
            curves.Add(new SimpleCurve(pointsA));
        }

        public Color Evaluate(float position)
        {
            return new Color(curves[0].Evaluate(position), curves[1].Evaluate(position), curves[2].Evaluate(position), curves[3].Evaluate(position));
        }
    }

    public struct GradientPoint
    {
        public float position;
        public Color color;
    }
}
