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
    public class ApparelGraphicPackage
    {
        // Graphic data for this package.
        public GraphicData graphicData;
        // List of offsets. Specific for every direction if 4 are specified, else applies to all directions
        // Y dimension determines layer offset, use negative values to render the texture below the pawn
        public List<Vector3> offsets;
        // When set to true, graphic will change with the owner's bodytype, similarly to apparel
        public bool useBodytype = false;

        // Coloring for the first and second masks respectively
        // In case chosen shader does not support masks, first mask acts as color
        public ApparelPackageColor firstMask = ApparelPackageColor.None;
        public ApparelPackageColor secondMask = ApparelPackageColor.None;

        private Dictionary<BodyTypeDef, Graphic> cachedGraphics = new Dictionary<BodyTypeDef, Graphic>();
        private Graphic cachedGraphic;

        private Color cachedFirstColor;
        private Color cachedSecondColor;

        public virtual Graphic GetGraphic(Apparel apparel, BodyTypeDef bodyType)
        {
            if (!useBodytype || bodyType == null)
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

    public enum ApparelPackageColor
    {
        None,
        ApparelColor, //Apparel's material color
        FactionColor,
        IdeoColor,
        FavoriteColor
    }
}
