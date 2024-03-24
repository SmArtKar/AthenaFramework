using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_DisplayFirefly : DroneComp
    {
        private DroneCompProperties_DisplayFirefly Props => props as DroneCompProperties_DisplayFirefly;

        public Vector2 position;
        public QuadBezierCurve curve;
        public bool above = false;
        public bool outbounds = false;

        public override void OnDeployed()
        {
            base.OnDeployed();
            position = Vector2.zero;
            above = true;
            outbounds = false;

            curve = new QuadBezierCurve();

            for (int i = 0; i < 3; i++)
            {
                curve.points.Add(new CurvePoint(Props.range.RandomInRange, Props.range.RandomInRange));
            }

            curve.StartMovement(Props.speed);
        }

        public override Vector3 DrawPosOffset()
        {
            if (parent.DisableHoveringAnimation())
            {
                return Vector3.zero;
            }

            return new Vector3(position.x, (above ? 0.2f : -0.2f) - parent.def.defaultLayer.AltitudeFor(), position.y);
        }

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (parent.DisableHoveringAnimation())
            {
                return;
            }

            position = curve.ContinueWithRandom(Props.range, Props.pointLimits);

            if (Math.Abs(position.x) > Props.pawnBounds.x && Math.Abs(position.y) > Props.pawnBounds.y)
            {
                if (!outbounds)
                {
                    outbounds = true;
                    above = Rand.Chance(0.5f);
                }
            }
            else
            {
                outbounds = false;
            }
        }
    }

    public class DroneCompProperties_DisplayFirefly : DroneCompProperties
    {
        // Range (in tiles) in which new destination points would be generated
        public FloatRange range = new FloatRange(-0.75f, 0.75f);
        // Distance limit (in tiles) after which the curve is cut in half to avoid overextension
        public Vector2 pointLimits = new Vector2(1f, 1f);
        // Speed at which the drone orbits the pawn, 1 being a full tile per second
        public float speed = 1f;
        // Range after exiting which the drone has a chance to move behind the pawn
        public Vector2 pawnBounds = new Vector2(0.4f, 0.6f);
        // Chance at whcih the drone would swap sides (infront/behind the pawn) upon exiting the bounds
        public float sideSwapChance = 0.5f;

        public DroneCompProperties_DisplayFirefly()
        {
            compClass = typeof(DroneComp_DisplayFirefly);
        }
    }
}
