using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    [StaticConstructorOnStartup]
    public class Gizmo_DroneHealthStatus : Gizmo
    {
        public Drone drone;

        public static readonly Texture2D FullHealthBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        public static readonly Texture2D EmptyHealthBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        static Gizmo_DroneHealthStatus() { }

        public Gizmo_DroneHealthStatus(Drone drone)
        {
            Order = -101f;
            this.drone = drone;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect backgroundRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Rect drawRect = backgroundRect.ContractedBy(6f);
            Widgets.DrawWindowBackground(backgroundRect);
            Rect textRect = drawRect;
            textRect.height = backgroundRect.height / 2f - 12f;
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(textRect, drone.LabelCap + (drone.broken ? " " + drone.def.brokenLabel : ""));
            Rect barRect = drawRect;
            barRect.yMin = drawRect.y + drawRect.height / 2f;
            float num = Math.Min(drone.health, drone.MaxHealth) / drone.MaxHealth;
            Widgets.FillableBar(barRect, num, Gizmo_DroneHealthStatus.FullHealthBarTex, Gizmo_DroneHealthStatus.EmptyHealthBarTex, false);
            Text.Font = GameFont.Small;
            Widgets.Label(barRect, (drone.health).ToString("F0") + " / " + (drone.MaxHealth).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(drawRect, drone.def.description);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
