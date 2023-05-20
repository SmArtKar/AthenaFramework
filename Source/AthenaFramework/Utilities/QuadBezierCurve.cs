using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace AthenaFramework
{
    public static class CurveUtility
    {
        public static Vector2 ToVector2(this CurvePoint point)
        {
            return new Vector2(point.x, point.y);
        }
    }

    public class QuadBezierCurve
    {
        public List<CurvePoint> points = new List<CurvePoint>();
        public float speed = 1f;
        public float currentPosition = 0f;

        public CurvePoint this[int i]
        {
            get
            {
                return points[i];
            }
            set
            {
                points[i] = value;
            }
        }

        public float Length
        {
            get
            {
                return (points[1].ToVector2() - points[0].ToVector2()).magnitude + (points[2].ToVector2() - points[1].ToVector2()).magnitude;
            }
        }

        public QuadBezierCurve(IEnumerable<CurvePoint> points)
        {
            SetPoints(points);
        }

        public QuadBezierCurve() { }

        public void SetPoints(IEnumerable<CurvePoint> newPoints)
        {
            points.Clear();

            foreach (CurvePoint newPoint in newPoints)
            {
                points.Add(newPoint);
            }
        }

        public Vector2 Evaluate(float t)
        {
            float x = points[0].x * (1 - t) * (1 - t) + 2 * t * (1 - t) * points[1].x + t * t * points[2].x;
            float y = points[0].y * (1 - t) * (1 - t) + 2 * t * (1 - t) * points[1].y + t * t * points[2].y;

            return new Vector2(x, y);
        }

        public void StartMovement(float speed)
        {
            this.speed = speed;
        }

        public void ContinueCurve(CurvePoint newPoint, Vector2 limits, bool firstHalf = false)
        {
            if (firstHalf)
            {
                points.RemoveAt(2);
                points.Add(newPoint);
                return;
            }

            Vector2 firstVector = 2 * points[2].ToVector2() - points[1].ToVector2();
            float x = firstVector.x;
            float y = firstVector.y;

            if (Math.Abs(x) > limits.x)
            {
                x /= 2f;
            }

            if (Math.Abs(y) > limits.y)
            {
                y /= 2f;
            }

            CurvePoint firstPoint = new CurvePoint(x, y);
            CurvePoint lastPoint = points[2];
            points.Clear();
            points.Add(lastPoint);
            points.Add(firstPoint);
            points.Add(newPoint);
        }

        public Vector2 ContinueWithRandom(FloatRange range, Vector2 limits)
        {
            currentPosition += speed / (60f * Length);

            if (currentPosition > 1)
            {
                currentPosition = 0f;
                ContinueCurve(new CurvePoint(range.RandomInRange, range.RandomInRange), limits);
            }

            return Evaluate(currentPosition);
        }

        public void SnapContinue(CurvePoint newPoint)
        {
            currentPosition = 0f;

            List<CurvePoint> newPoints = new List<CurvePoint>();
            newPoints.Add(new CurvePoint(Evaluate(Math.Max(0, currentPosition - speed / (6f * Length)))));
            newPoints.Add(new CurvePoint(Evaluate(currentPosition)));
            newPoints.Add(newPoint);
            points = newPoints;
        }
    }
}
