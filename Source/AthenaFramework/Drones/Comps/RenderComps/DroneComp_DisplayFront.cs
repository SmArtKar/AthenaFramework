using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_DisplayFront : DroneComp
    {
        private DroneCompProperties_DisplayFront Props => props as DroneCompProperties_DisplayFront;

        public int dronePlace = -1;
        public DroneComp_DisplayFront parentDroneComp = null;

        public virtual Vector2 DronePosition
        {
            get
            {
                Vector2 directOffset = new Vector2(0, Props.pawnSize.x * Pawn.Rotation.AsVector2.x);

                if (Pawn.Rotation == Rot4.North || Pawn.Rotation == Rot4.South)
                {
                    directOffset = new Vector2(0, Props.pawnSize.y * Pawn.Rotation.AsVector2.y);
                }

                if (dronePlace == 0)
                {
                    return directOffset;
                }

                bool droneSide = dronePlace % 2 == 1;

                if (Pawn.Rotation == Rot4.West || Pawn.Rotation == Rot4.South)
                {
                    droneSide = !droneSide;
                }

                if (Pawn.Rotation == Rot4.North || Pawn.Rotation == Rot4.South)
                {
                    return new Vector2(Props.droneSize.x * (droneSide ? -1 : 1), 0) + parentDroneComp.DronePosition;
                }

                return new Vector2(0, Props.droneSize.y * (droneSide ? -1 : 1)) + parentDroneComp.DronePosition;
            }
        }

        public override void OnRecalled()
        {
            base.OnRecalled();
            dronePlace = -1;
        }

        public override Vector3 DrawPosOffset()
        {
            if (parent.DisableHoveringAnimation())
            {
                return Vector3.zero;
            }

            if (!Props.changeLayers)
            {
                return new Vector3(DronePosition.x, 0, DronePosition.y);
            }

            if (dronePlace == 0 || Pawn.Rotation == Rot4.South || Pawn.Rotation == Rot4.North)
            {
                return new Vector3(DronePosition.x, (Pawn.Rotation == Rot4.North ? -0.2f : 0.2f) - parent.def.defaultLayer.AltitudeFor(), DronePosition.y);
            }

            bool droneSide = dronePlace % 2 == 1;

            if (Pawn.Rotation == Rot4.West)
            {
                droneSide = !droneSide;
            }

            return new Vector3(DronePosition.x, (droneSide ? -0.2f - dronePlace * 0.01f : 0.2f + dronePlace * 0.01f) - parent.def.defaultLayer.AltitudeFor(), DronePosition.y + Props.height);
        }

        public override void OnDeployed()
        {
            base.OnDeployed();

            List<DroneComp_DisplayFront> otherComps = new List<DroneComp_DisplayFront>();
            parentDroneComp = null;

            for (int i = Pawn.health.hediffSet.hediffs.Count; i >= 0; i--)
            {
                Hediff_DroneHandler handler = Pawn.health.hediffSet.hediffs[i] as Hediff_DroneHandler;

                if (handler == null)
                {
                    continue;
                }

                Drone drone = handler.drone;

                if (!drone.active)
                {
                    continue;
                }

                DroneComp_DisplayFront comp = drone.TryGetComp<DroneComp_DisplayFront>();

                if (comp != null)
                {
                    otherComps.Add(comp);
                }
            }

            dronePlace = otherComps.Count;

            if (dronePlace > 0)
            {
                for (int i = otherComps.Count - 1; i >= 0; i--)
                {
                    if (otherComps[i].dronePlace == Math.Max(dronePlace - 2, 0))
                    {
                        parentDroneComp = otherComps[i];
                    }
                }
            }
        }
    }

    public class DroneCompProperties_DisplayFront : DroneCompProperties
    {
        // What initial offset should the drone have if its in the center
        public Vector2 pawnSize = new Vector2(0.5f, 0.3f);
        // What offset should this drone have if its not directly infront of the pawn
        public Vector2 droneSize = new Vector2(0.3f, 0.3f);
        // Default vertical offset of the drone
        public float height = 0.2f;
        // Whenever the drone should go infront/behind the pawn or always stay ontop
        public bool changeLayers = true;

        public DroneCompProperties_DisplayFront()
        {
            compClass = typeof(DroneComp_DisplayFront);
        }
    }
}
