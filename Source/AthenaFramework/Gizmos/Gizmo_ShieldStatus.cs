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
    public class Gizmo_ShieldStatus : Gizmo
    {
        public Comp_ShieldEquipment shieldComp;

        public static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        public static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);

        private CompProperties_ShieldEquipment props => shieldComp.props as CompProperties_ShieldEquipment;

        static Gizmo_ShieldStatus() { }

        public Gizmo_ShieldStatus(Comp_ShieldEquipment shield)
        {
            Order = -101f;
            shieldComp = shield;
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
            Widgets.Label(textRect, props.gizmoTitle);
            Rect barRect = drawRect;
            barRect.yMin = drawRect.y + drawRect.height / 2f;
            float num = shieldComp.energy / Mathf.Max(1f, shieldComp.MaxEnergy);
            Widgets.FillableBar(barRect, num, Gizmo_HediffShieldStatus.FullShieldBarTex, Gizmo_HediffShieldStatus.EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Widgets.Label(barRect, (shieldComp.energy).ToString("F0") + " / " + (shieldComp.MaxEnergy).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(drawRect, props.gizmoTip);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
