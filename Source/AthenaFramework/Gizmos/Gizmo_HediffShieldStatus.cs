using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework.Gizmos
{
    [StaticConstructorOnStartup]
    public class Gizmo_HediffShieldStatus : Gizmo
    {
        public HediffComp_Shield shieldHediff;

        private static readonly Texture2D FullShieldBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.2f, 0.24f));
        private static readonly Texture2D EmptyShieldBarTex = SolidColorMaterials.NewSolidColorTexture(Color.clear);
        static Gizmo_HediffShieldStatus() { }

        public Gizmo_HediffShieldStatus()
        {
            Order = -101f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            HediffCompProperties_Shield props = shieldHediff.props as HediffCompProperties_Shield;
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
            float num = shieldHediff.energy / Mathf.Max(1f, props.maxEnergy);
            Widgets.FillableBar(barRect, num, Gizmo_HediffShieldStatus.FullShieldBarTex, Gizmo_HediffShieldStatus.EmptyShieldBarTex, false);
            Text.Font = GameFont.Small;
            Widgets.Label(barRect, (shieldHediff.energy).ToString("F0") + " / " + (props.maxEnergy).ToString("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
            TooltipHandler.TipRegion(drawRect, props.gizmoTip);
            return new GizmoResult(GizmoState.Clear);
        }
    }
}
