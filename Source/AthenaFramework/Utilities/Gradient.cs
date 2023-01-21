using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class GradientCurve
    {
        public List<GradientPoint> points;
        public List<SimpleCurve> curves = new List<SimpleCurve>();

        public GradientCurve(List<GradientPoint> points)
        {
            this.points = points;
            GenerateCurves();
        }

        public virtual void GenerateCurves()
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

        public virtual Color Evaluate(float position)
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
