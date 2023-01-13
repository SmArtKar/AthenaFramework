using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace AthenaFramework
{
    public class Comp_AdditionalApparelGraphics : ThingComp
    {
        private CompProperties_AdditionalApparelGraphics Props => props as CompProperties_AdditionalApparelGraphics;
        private Apparel Apparel => parent as Apparel;
        private Pawn Pawn => Apparel.Wearer as Pawn;

        public virtual void DrawAt(Vector3 drawPos)
        {
            List<Apparel> wornApparel = Pawn.apparel.WornApparel;
            BodyTypeDef bodyType = Pawn.story.bodyType;

            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                Apparel otherApparel = wornApparel[i];

                for (int j = otherApparel.AllComps.Count - 1; j >= 0; j--)
                {
                    Comp_CustomApparelBody customBody = otherApparel.AllComps[j] as Comp_CustomApparelBody;

                    if (customBody == null)
                    {
                        continue;
                    }

                    BodyTypeDef newBodyType = customBody.CustomBodytype(Apparel, bodyType);

                    if (newBodyType != null)
                    {
                        bodyType = newBodyType;
                    }
                }
            }

            for (int i = Props.additionalGraphics.Count - 1; i >= 0; i--)
            {
                ApparelGraphicPackage package = Props.additionalGraphics[i];
                Vector3 offset = new Vector3();

                if (package.offsets != null)
                {
                    if (package.offsets.Count == 4)
                    {
                        offset = package.offsets[Pawn.Rotation.AsInt];
                    }
                    else
                    {
                        offset = package.offsets[0];
                    }
                }

                package.GetGraphic(Apparel, bodyType).Draw(drawPos + offset, Pawn.Rotation, Pawn);
            }
        }
    }

    public class CompProperties_AdditionalApparelGraphics : CompProperties
    {
        public CompProperties_AdditionalApparelGraphics()
        {
            this.compClass = typeof(Comp_AdditionalApparelGraphics);
        }

        // Additional graphic layers with precise controls
        public List<ApparelGraphicPackage> additionalGraphics = new List<ApparelGraphicPackage>();
    }

    public enum ApparelPackageColor
    {
        None,
        ApparelColor, //Apparel's color
        FactionColor,
        IdeoColor,
        FavoriteColor
    }

    public class ApparelGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;
        // When set to true, graphic will change with the owner's bodytype, similarly to apparel
        public bool useBodytype = false;

        // Coloring for the first and second masks respectively. Only firstMask supports gradients
        // In case chosen shader does not support masks, first mask acts as color
        public ApparelPackageColor firstMask = ApparelPackageColor.None;
        public ApparelPackageColor secondMask = ApparelPackageColor.None;

        private Dictionary<BodyTypeDef, Graphic> cachedGraphics = new Dictionary<BodyTypeDef, Graphic>();
        private Graphic cachedGraphic; //GraphicDatabase.Get<Graphic_StackCount>(this.path, newShader, this.drawSize, newColor, newColorTwo, this.data, null);

        private Color cachedFirstColor;
        private Color cachedSecondColor;

        public virtual Graphic GetGraphic(Apparel apparel, BodyTypeDef bodyType)
        {
            if (!useBodytype)
            {
                Color firstColor1 = GetColor(firstMask, apparel) ?? graphicData.color;
                Color secondColor1 = GetColor(secondMask, apparel) ?? graphicData.colorTwo;

                if (firstColor1 == cachedFirstColor && secondColor1 == cachedSecondColor)
                {
                    return cachedGraphic;
                }

                cachedFirstColor = firstColor1;
                cachedSecondColor = secondColor1;

                cachedGraphic = graphicData.Graphic.GetColoredVersion(graphicData.Graphic.Shader, firstColor1, secondColor1);

                return cachedGraphic;
            }

            Color firstColor2 = GetColor(firstMask, apparel) ?? graphicData.color;
            Color secondColor2 = GetColor(secondMask, apparel) ?? graphicData.colorTwo;

            if (firstColor2 != cachedFirstColor || secondColor2 != cachedSecondColor || !cachedGraphics.ContainsKey(bodyType))
            {
                cachedGraphics[bodyType] = GraphicDatabase.Get(graphicData.graphicClass, graphicData.texPath + "_" + bodyType.defName, graphicData.Graphic.Shader, graphicData.drawSize, firstColor2, secondColor2);
            }

            cachedFirstColor = firstColor2;
            cachedSecondColor = secondColor2;

            return cachedGraphics[bodyType];
        }

        public virtual Color? GetColor(ApparelPackageColor type, Apparel apparel)
        {
            switch (type)
            {
                case ApparelPackageColor.ApparelColor:
                    return apparel.DrawColor;

                case ApparelPackageColor.FactionColor:
                    if (apparel.Wearer == null || apparel.Wearer.Faction == null)
                    {
                        return null;
                    }

                    return apparel.Wearer.Faction.Color;

                case ApparelPackageColor.IdeoColor:
                    if (!ModLister.IdeologyInstalled || apparel.Wearer == null || apparel.Wearer.ideo == null)
                    {
                        return null;
                    }

                    return apparel.Wearer.ideo.Ideo.Color;

                case ApparelPackageColor.FavoriteColor:
                    if (!ModLister.IdeologyInstalled || apparel.Wearer == null || apparel.Wearer.story == null)
                    {
                        return null;
                    }

                    return apparel.Wearer.story.favoriteColor;
            }

            return null;
        }
    }
}
