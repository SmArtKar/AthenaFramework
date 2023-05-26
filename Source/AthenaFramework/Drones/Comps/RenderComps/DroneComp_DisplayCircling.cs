using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class DroneComp_DisplayCircling : DroneComp
    {
        private DroneCompProperties_DisplayCircling Props => props as DroneCompProperties_DisplayCircling;

        public float angle;

        public override void OnDeployed()
        {
            base.OnDeployed();
            angle = 0;
        }

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (!parent.DisableHoveringAnimation())
            {
                angle += Props.speed / 60f;
            }
        }

        public override Vector3 DrawPosOffset()
        {
            if (parent.DisableHoveringAnimation())
            {
                return Vector3.zero;
            }

            float horizontal = (float)Math.Cos(angle);
            float vertical = (float)Math.Sin(angle);
            
            if (Pawn.Rotation == Rot4.North || Pawn.Rotation == Rot4.South)
            {
                horizontal += vertical;
                vertical = horizontal - vertical;
                horizontal -= vertical;
            }

            return new Vector3(horizontal * Props.orbitWidth, Props.changeLayers ? (((vertical < 1) ? 0.2f : -0.2f) - parent.def.defaultLayer.AltitudeFor()) : 0, vertical * Props.orbitHeight + Props.height);
        }
    }

    public class DroneCompProperties_DisplayCircling : DroneCompProperties
    {
        // Speed in radians per second
        public float speed = 1f;
        // Vertical offset for the drone, in tiles
        public float height = 0f;
        // Width of the drone's orbit radius, in tiles
        public float orbitWidth = 0.5f;
        // How squished the drone orbit is. Setting this to 0 will make it orbit purely horizontally
        public float orbitHeight = 0.2f;
        // Whenever the drone should go infront/behind the pawn or always stay ontop
        public bool changeLayers = true;

        public DroneCompProperties_DisplayCircling()
        {
            compClass = typeof(DroneComp_DisplayCircling);
        }
    }
}
