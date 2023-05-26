using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_DisplayShoulder : DroneComp
    {
        private DroneCompProperties_DisplayShoulder Props => props as DroneCompProperties_DisplayShoulder;

        public int dronePlace = -1;
        public DroneComp_DisplayShoulder parentDroneComp = null;

        public virtual Vector2 DronePosition
        {
            get
            {
                bool droneSide = dronePlace % 2 == 1;

                if (Pawn.Rotation == Rot4.West || Pawn.Rotation == Rot4.South)
                {
                    droneSide = !droneSide;
                }

                if (dronePlace < 2)
                {
                    if (Pawn.Rotation == Rot4.North || Pawn.Rotation == Rot4.South)
                    {
                        return new Vector2(Props.pawnSize.x * (droneSide ? -1 : 1), 0);
                    }

                    return new Vector2(0, Props.pawnSize.y * (droneSide ? -1 : 1));
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

            if (Pawn.Rotation == Rot4.North || Pawn.Rotation == Rot4.South || !Props.changeLayers)
            {
                return new Vector3(DronePosition.x, 0, DronePosition.y + Props.height);
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

            List<DroneComp_DisplayShoulder> otherComps = new List<DroneComp_DisplayShoulder>();
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

                DroneComp_DisplayShoulder comp = drone.TryGetComp<DroneComp_DisplayShoulder>();

                if (comp != null)
                {
                    otherComps.Add(comp);
                }
            }

            dronePlace = otherComps.Count;

            if (dronePlace > 1)
            {
                for (int i = otherComps.Count - 1; i >= 0; i--)
                {
                    if (otherComps[i].dronePlace == dronePlace - 2)
                    {
                        parentDroneComp = otherComps[i];
                    }
                }
            }
        }
    }

    public class DroneCompProperties_DisplayShoulder : DroneCompProperties
    {
        // What initial offset should the drone have if it follows a pawn
        public Vector2 pawnSize = new Vector2(0.5f, 0.3f);
        // What offset should this drone have if it follows another drone
        public Vector2 droneSize = new Vector2(0.3f, 0.3f);
        // Default vertical offset of the drone
        public float height = 0.2f;
        // Whenever the drone should go infront/behind the pawn or always stay ontop
        public bool changeLayers = true;

        public DroneCompProperties_DisplayShoulder()
        {
            compClass = typeof(DroneComp_DisplayShoulder);
        }
    }
}
