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
    public class Comp_AdditionalApparelGraphics : ThingComp, IRenderable
    {
        private CompProperties_AdditionalApparelGraphics Props => props as CompProperties_AdditionalApparelGraphics;
        private Apparel Apparel => parent as Apparel;
        private Pawn Pawn => Apparel.Wearer as Pawn;

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
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
}
