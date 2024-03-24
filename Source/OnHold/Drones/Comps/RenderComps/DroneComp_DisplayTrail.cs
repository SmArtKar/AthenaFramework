using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_DisplayTrail : DroneComp
    {
        private DroneCompProperties_DisplayTrail Props => props as DroneCompProperties_DisplayTrail;

        public int dronePlace = -1;
        public Vector2 position;
        public DroneComp_DisplayTrail parentDroneComp = null;
        public bool animationDisabled = false;

        public virtual Vector2 TargetDronePosition
        {
            get
            {
                if (dronePlace == 0)
                {
                    return Pawn.Rotation.Opposite.AsVector2 * ((Pawn.Rotation == Rot4.East || Pawn.Rotation == Rot4.West) ? Props.pawnSize.x : Props.pawnSize.y);
                }

                return parentDroneComp.TargetDronePosition + Pawn.Rotation.Opposite.AsVector2 * ((Pawn.Rotation == Rot4.East || Pawn.Rotation == Rot4.West) ? Props.droneSize.x : Props.droneSize.y);
            }
        }

        public override void OnRecalled()
        {
            base.OnRecalled();
            dronePlace = -1;
        }

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (parent.DisableHoveringAnimation())
            {
                animationDisabled = true;
                return;
            }

            if (animationDisabled)
            {
                animationDisabled = false;
                position = new Vector2(parent.DrawPos.x, parent.DrawPos.z);
            }

            Vector2 targetPosition = new Vector2(Pawn.Position.x, Pawn.Position.z) + TargetDronePosition;
            Vector2 movement = Vector2.ClampMagnitude((targetPosition - position).normalized * Props.speed / 60f, (targetPosition - position).magnitude);

            position += movement;
        }

        public override void OnDeployed()
        {
            base.OnDeployed();

            List<DroneComp_DisplayTrail> otherComps = new List<DroneComp_DisplayTrail>();
            parentDroneComp = null;
            position = new Vector2(Pawn.Position.x, Pawn.Position.z);

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

                DroneComp_DisplayTrail comp = drone.TryGetComp<DroneComp_DisplayTrail>();

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

        public override Vector3 DrawPosOffset()
        {
            if (parent.DisableHoveringAnimation())
            {
                return Vector3.zero;
            }

            return new Vector3(position.x, Props.changeLayers ? (((position.y < Pawn.Position.y) ? 0.2f + dronePlace * 0.01f : -0.2f - dronePlace * 0.01f) - parent.def.defaultLayer.AltitudeFor()) : 0, position.y);
        }
    }

    public class DroneCompProperties_DisplayTrail : DroneCompProperties
    {
        // What initial offset should the drone have if it follows a pawn
        public Vector2 pawnSize = new Vector2(0.5f, 0.3f);
        // What offset should this drone have if it follows another drone
        public Vector2 droneSize = new Vector2(0.3f, 0.3f);
        // Default vertical offset of the drone
        public float height = 0.2f;
        // Whenever the drone should go infront/behind the pawn or always stay ontop
        public bool changeLayers = true;
        // Speed at which the drone catches up to the pawn, tiles per second
        public float speed = 1f;

        public DroneCompProperties_DisplayTrail()
        {
            compClass = typeof(DroneComp_DisplayTrail);
        }
    }
}
