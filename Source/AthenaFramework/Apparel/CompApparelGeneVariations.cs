using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class CompApparelGeneVariations : ThingComp, IBodyModifier
    {
        private CompProperties_ApparelGeneVariations Props => props as CompProperties_ApparelGeneVariations;
        protected Apparel Apparel => parent as Apparel;
        protected Pawn Wearer => Apparel.Wearer;

        public bool HideBody => false;
        public bool HideHead => false;
        public bool HideHair => false;
        public bool HideFur => false;

        public bool CustomBodytype(ref BodyTypeDef bodyType) { return false; }
        public bool CustomHeadtype(ref HeadTypeDef headType) { return false; }
        public virtual void FurGraphic(ref Graphic furGraphic) { }

        public bool CustomApparelTexture(BodyTypeDef bodyType, Apparel apparel, ref ApparelGraphicRecord rec)
        {
            ApparelGeneVariationPackage package = null;
            
            for (int i = Props.variations.Count - 1; i >= 0; i--)
            {
                package = Props.variations[i];

                if (package.ShouldActivate(Wearer))
                {
                    break;
                }

                package = null;
            }

            if (package == null)
            {
                return false;
            }

            Shader shader = ShaderDatabase.Cutout;
            if (apparel.def.apparel.useWornGraphicMask)
            {
                shader = ShaderDatabase.CutoutComplex;
            }

            string path = package.wornTexPath;

            if (package.useBodytypes && apparel.def.apparel.LastLayer != ApparelLayerDefOf.Overhead && apparel.def.apparel.LastLayer != ApparelLayerDefOf.EyeCover && !PawnRenderUtility.RenderAsPack(apparel) && apparel.WornGraphicPath != BaseContent.PlaceholderImagePath && apparel.WornGraphicPath != BaseContent.PlaceholderGearImagePath)
            {
                path += "_" + bodyType.defName;
            }

            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(path, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
            rec = new ApparelGraphicRecord(graphic, apparel);

            return true;
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            AthenaCache.AddCache(this, ref AthenaCache.bodyCache, pawn.thingIDNumber);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            AthenaCache.RemoveCache(this, AthenaCache.bodyCache, pawn.thingIDNumber);
            pawn.Drawer.renderer.SetAllGraphicsDirty();
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
            {
                return;
            }
            if (Wearer != null)
            {
                AthenaCache.AddCache(this, ref AthenaCache.bodyCache, Wearer.thingIDNumber);
                Wearer.Drawer.renderer.SetAllGraphicsDirty();
            }
        }
    }

    public class CompProperties_ApparelGeneVariations : CompProperties
    {
        public List<ApparelGeneVariationPackage> variations;

        public CompProperties_ApparelGeneVariations()
        {
            this.compClass = typeof(CompApparelGeneVariations);
        }
    }

    public class ApparelGeneVariationPackage
    {
        // Genes and xenotype required to activate this package
        public List<GeneDef> genes;
        public XenotypeDef xenotype;

        // Alternate texture path
        public string wornTexPath;
        // If set to true, then this variation would also use bodytypes
        public bool useBodytypes = false;

        public bool ShouldActivate(Pawn pawn)
        {
            if (pawn.genes.xenotype == xenotype)
            {
                return true;
            }

            List<GeneDef> remainingDefs = new List<GeneDef>(genes);

            for (int i = pawn.genes.GenesListForReading.Count - 1; i >= 0; i--)
            {
                Gene gene = pawn.genes.GenesListForReading[i];

                if (remainingDefs.Contains(gene.def))
                {
                    remainingDefs.Remove(gene.def);
                }
            }

            return remainingDefs.Count == 0;
        }
    }
}
