using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class Dialog_ColorPalette : Window
    {
        public IColorSelector linkedObject;

        private static readonly FloatRange range = new FloatRange(0f, 255f);

        public float red1;
        public float green1;
        public float blue1;

        public float red2;
        public float green2;
        public float blue2;

        private static readonly Vector2 size1 = new Vector2(544f, 252f);
        private static readonly Vector2 size2 = new Vector2(544f, 141f);

        public Dialog_ColorPalette(IColorSelector linked)
        {
            linkedObject = linked;

            red1 = linkedObject.PrimaryColor.r * 255f;
            green1 = linkedObject.PrimaryColor.g * 255f;
            blue1 = linkedObject.PrimaryColor.b * 255f;

            red2 = linkedObject.SecondaryColor.r * 255f;
            green2 = linkedObject.SecondaryColor.g * 255f;
            blue2 = linkedObject.SecondaryColor.b * 255f;
        }

        public override Vector2 InitialSize
        {
            get
            {
                return linkedObject.UseSecondary ? size1 : size2;
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Color PrimaryColor = linkedObject.PrimaryColor;
            Color SecondaryColor = linkedObject.SecondaryColor;

            Rect firstColorRect = new Rect(inRect.x + 10f, inRect.y + 10f, 84f, 84f);

            GUI.color = PrimaryColor;
            GUI.DrawTexture(firstColorRect, BaseContent.WhiteTex);
            GUI.color = Color.white;

            Rect sliderR1 = new Rect(inRect.x + 114f, inRect.y + 10f, 384f, 16f);
            Widgets.HorizontalSlider(sliderR1, ref red1, range, label: "Primary Red");

            Rect sliderG1 = new Rect(inRect.x + 114f, inRect.y + 40f, 384f, 16f);
            Widgets.HorizontalSlider(sliderG1, ref green1, range, label: "Primary Green");

            Rect sliderB1 = new Rect(inRect.x + 114f, inRect.y + 70f, 384f, 16f);
            Widgets.HorizontalSlider(sliderB1, ref blue1, range, label: "Primary Blue");

            if (red1 / 255f != PrimaryColor.r || green1 / 255f != PrimaryColor.g || blue1 / 255f != PrimaryColor.b)
            {
                linkedObject.PrimaryColor = new Color(red1 / 255f, green1 / 255f, blue1 / 255f);
            }

            Rect secondColorRect = new Rect(inRect.x + 10f, inRect.y + 10f + 30f + 84f, 84f, 84f);

            GUI.color = SecondaryColor;
            GUI.DrawTexture(secondColorRect, BaseContent.WhiteTex);
            GUI.color = Color.white;

            Rect sliderR2 = new Rect(inRect.x + 114f, inRect.y + 124f, 384f, 16f);
            Widgets.HorizontalSlider(sliderR2, ref red2, range, label: "Secondary Red");

            Rect sliderG2 = new Rect(inRect.x + 114f, inRect.y + 154f, 384f, 16f);
            Widgets.HorizontalSlider(sliderG2, ref green2, range, label: "Secondary Green");

            Rect sliderB2 = new Rect(inRect.x + 114f, inRect.y + 184f, 384f, 16f);
            Widgets.HorizontalSlider(sliderB2, ref blue2, range, label: "Secondary Blue");

            if (red2 / 255f != SecondaryColor.r || green2 / 255f != SecondaryColor.g || blue2 / 255f != SecondaryColor.b)
            {
                linkedObject.SecondaryColor = new Color(red2 / 255f, green2 / 255f, blue2 / 255f);
            }
        }
    }
}
