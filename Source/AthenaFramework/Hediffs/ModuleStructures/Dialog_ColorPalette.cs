using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace AthenaFramework
{
    public class Dialog_ColorPalette : Window
    {
        public IColorSelector linkedObject;

        private static readonly FloatRange range = new FloatRange(0f, 255f);
        private static readonly Vector2 size1 = new Vector2(600f, 450f);
        private static readonly Vector2 size2 = new Vector2(600f, 450f);

        private bool colorWheelDragging;
        private string[] textfieldBuffers = new string[6];
        private Color textfieldColorBuffer;
        private string previousFocusedControlName;
        private string hexBuffer;

        public Color primaryColor;
        public Color secondaryColor;
        public Color oldPrimary;
        public Color oldSecondary;
        public Color editColor;

        bool usePrimary = true;

        public static List<Color> colors = null;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(600f, Math.Max(435f, ((int)(colors.Count / 9)) * 24f + 128f));
            }
        }

        public Dialog_ColorPalette(IColorSelector linked)
        {
            if (colors == null)
            {
                colors = new List<Color>(Dialog_GlowerColorPicker.colors);
            }

            linkedObject = linked;

            primaryColor = new Color(linked.PrimaryColor.r, linked.PrimaryColor.g, linked.PrimaryColor.b);
            secondaryColor = new Color(linked.SecondaryColor.r, linked.SecondaryColor.g, linked.SecondaryColor.b);

            oldPrimary = primaryColor;
            oldSecondary = secondaryColor;

            editColor = primaryColor;

            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            closeOnAccept = false;
        }

        public void HeaderRow(ref RectDivider layout)
        {
            using (new TextBlock(GameFont.Medium))
            {
                TaggedString taggedString = "ChooseAColor".Translate().CapitalizeFirst();
                GUI.SetNextControlName("title");
                RectDivider row = layout.NewRow(Text.CalcHeight(taggedString, layout.Rect.width) + 4f);
                Widgets.Label(row, taggedString);

                if (!linkedObject.UseSecondary)
                {
                    return;
                }

                row = row.NewRow(row.Rect.height - 4f);

                using (new TextBlock(GameFont.Small))
                {
                    if (Widgets.ButtonText(row.NewCol(150f, HorizontalJustification.Right), "Secondary color"))
                    {
                        editColor = secondaryColor;
                        usePrimary = false;
                    }

                    if (Widgets.ButtonText(row.NewCol(150f, HorizontalJustification.Right), "Primary color"))
                    {
                        editColor = primaryColor;
                        usePrimary = true;
                    }
                }
            }
        }

        public void BottomButtons(ref RectDivider layout)
        {
            RectDivider rectDivider = layout.NewRow(38f, VerticalJustification.Bottom);

            if (Widgets.ButtonText(rectDivider.NewCol(150f), "Cancel".Translate()))
            {
                Close();
            }

            if (Widgets.ButtonText(rectDivider.NewCol(150f), "Add to palette"))
            {
                colors.Add(usePrimary ? primaryColor : secondaryColor);
            }

            if (Widgets.ButtonText(rectDivider.NewCol(150f, HorizontalJustification.Right), "Accept".Translate()))
            {
                linkedObject.PrimaryColor = primaryColor;
                linkedObject.SecondaryColor = secondaryColor;
                Close();
            }
        }

        public void ColorPalette(ref RectDivider layout, ref Color color)
        {
            using (new TextBlock(TextAnchor.MiddleLeft))
            {
                RectDivider rectDivider = layout;
                Widgets.ColorSelector(rectDivider, ref color, colors, out float paletteHeight);
                rectDivider.NewRow(paletteHeight - 4f + 2f);
                int num = 26;
                RectDivider rectDivider3 = rectDivider.NewRow(num);
                int num2 = 4;
                rectDivider3.Rect.SplitVertically(num2 * (num + 2), out var left, out var right);
                paletteHeight += num + 2;
                RectDivider rectDivider4 = new RectDivider(left, 195906069, new Vector2(10f, 2f));
                RectDivider rectDivider5 = new RectDivider(right, 195906069, new Vector2(10f, 2f));
                Rect rect = rectDivider5.NewCol(num);
            }
        }

        public void ColorTextfields(ref RectDivider layout, ref Color color)
        {
            RectAggregator aggregator = new RectAggregator(new Rect(layout.Rect.position, new Vector2(125f, 0f)), 195906069);

            Widgets.ColorTextfields(ref aggregator, ref color, ref textfieldBuffers, ref textfieldColorBuffer, previousFocusedControlName, "colorTextfields", Widgets.ColorComponents.All, Widgets.ColorComponents.All); RectDivider rectDivider = aggregator.NewRow(30);
        }

        public void ColorReadback(Rect rect, Color color, Color oldColor)
        {
            RectDivider rectDivider = new RectDivider(rect, 195906069);
            TaggedString label = "CurrentColor".Translate().CapitalizeFirst();
            TaggedString label2 = "OldColor".Translate().CapitalizeFirst();
            float width = Mathf.Max(100f, label.GetWidthCached(), label2.GetWidthCached());
            RectDivider rectDivider2 = rectDivider.NewRow(Text.LineHeight);
            Widgets.Label(rectDivider2.NewCol(width), label);
            Widgets.DrawBoxSolid(rectDivider2, color);
            RectDivider rectDivider3 = rectDivider.NewRow(Text.LineHeight);
            Widgets.Label(rectDivider3.NewCol(width), label2);
            Widgets.DrawBoxSolid(rectDivider3, oldColor);
            RectDivider rectDivider4 = new RectDivider(rect, 195906069);
            rectDivider4.NewCol(26f);
        }

        public void HexPicker(ref RectDivider layout, ref Color color)
        {
            string hexString = "#" + ColorUtility.ToHtmlStringRGB(color);

            RectDivider divider = layout.NewRow(30f);

            string newHex = Widgets.DelayedTextField(divider.NewCol(127f), hexString, ref hexBuffer, previousFocusedControlName, "colorHexField");

            newHex.Remove(0);
            ColorUtility.TryParseHtmlString(newHex, out color);

            layout.NewRow(5f);
        }

        public override void DoWindowContents(Rect inRect)
        {
            using (TextBlock.Default())
            {
                RectDivider layout = new RectDivider(inRect, 195906069);
                HeaderRow(ref layout);
                BottomButtons(ref layout);
                layout.NewRow(0f, VerticalJustification.Bottom);
                RectDivider layout2 = layout.NewCol(250f, HorizontalJustification.Left);
                RectDivider layout3 = layout.NewCol(250f, HorizontalJustification.Right);

                if (usePrimary)
                {
                    ColorPalette(ref layout3, ref primaryColor);
                }
                else
                {
                    ColorPalette(ref layout3, ref secondaryColor);
                }

                RectDivider layout4 = layout2.NewRow(216f);
                RectDivider layout5 = layout4.NewCol(170f, HorizontalJustification.Left);

                if (usePrimary)
                {
                    ColorTextfields(ref layout5, ref primaryColor);
                }
                else
                {
                    ColorTextfields(ref layout5, ref secondaryColor);
                }

                Color.RGBToHSV(usePrimary ? primaryColor : secondaryColor, out float H, out float S, out float V);
                editColor = Color.HSVToRGB(H, S, 1f);
                editColor.a = 1f;
                Widgets.HSVColorWheel(layout4.Rect.ContractedBy((layout4.Rect.width - 128f) / 2f, (layout4.Rect.height - 128f) / 2f), ref editColor, ref colorWheelDragging, 1f);
                Color.RGBToHSV(usePrimary ? primaryColor : secondaryColor, out H, out S, out V);

                if (usePrimary)
                {
                    HexPicker(ref layout2, ref primaryColor);
                }
                else
                {
                    HexPicker(ref layout2, ref secondaryColor);
                }

                if (V > 1)
                {
                    V /= 100;
                }

                Color.RGBToHSV(editColor, out float H2, out float S2, out float V2);

                if (usePrimary)
                {
                    primaryColor = Color.HSVToRGB(H2, S2, V);
                }
                else
                {
                    secondaryColor = Color.HSVToRGB(H2, S2, V);
                }
                
                layout.NewRow(5f);

                ColorReadback(layout2, usePrimary ? primaryColor : secondaryColor, usePrimary ? oldPrimary : oldSecondary);
                if (Event.current.type == EventType.Layout)
                {
                    previousFocusedControlName = GUI.GetNameOfFocusedControl();
                }
            }
        }
    }
}
