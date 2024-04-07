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
    public class Comp_CustomApparelBody : ThingComp, IBodyModifier
    {
        private CompProperties_CustomApparelBody Props => props as CompProperties_CustomApparelBody;
        protected Apparel Apparel => parent as Apparel;
        protected Pawn Wearer => Apparel.Wearer;

        public virtual bool CustomApparelTexture(BodyTypeDef bodyType, Apparel apparel, ref ApparelGraphicRecord rec)
        {
            if (apparel.WornGraphicPath.NullOrEmpty())
            {
                return false;
            }

            Shader shader = ShaderDatabase.Cutout;
            if (apparel.def.apparel.useWornGraphicMask)
            {
                shader = ShaderDatabase.CutoutComplex;
            }

            Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.WornGraphicPath, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
            rec = new ApparelGraphicRecord(graphic, apparel);

            return true;
        }

        public virtual bool HideBody
        {
            get
            {
                return Props.hideBody;
            }
        }

        public virtual bool HideHead
        {
            get
            {
                return Props.hideHead;
            }
        }

        public virtual bool HideFur
        {
            get
            {
                return Props.hideFur;
            }
        }

        public virtual bool HideHair
        {
            get
            {
                return Props.hideHair;
            }
        }

        public virtual bool CustomBodytype(ref BodyTypeDef bodyType)
        {
            if (bodyType == null)
            {
                return false;
            }

            if (Props.forcedBodytype != null)
            {
                bodyType = Props.forcedBodytype;
                return true;
            }

            if (Props.bodytypePairs != null)
            {
                for (int i = Props.bodytypePairs.Count - 1; i >= 0; i--)
                {
                    BodytypeSwitch pair = Props.bodytypePairs[i];
                    if (pair.initialBodytype == bodyType)
                    {
                        bodyType = pair.newBodytype;
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool CustomHeadtype(ref HeadTypeDef headType)
        {
            if (headType == null)
            {
                return false;
            }

            if (Props.forcedHeadtype != null)
            {
                headType = Props.forcedHeadtype;
                return true;
            }

            if (Props.headtypePairs != null)
            {
                for (int i = Props.headtypePairs.Count - 1; i >= 0; i--)
                {
                    HeadtypeSwitch pair = Props.headtypePairs[i];
                    if (pair.initialHeadtype == headType)
                    {
                        headType = pair.newHeadtype;
                        return true;
                    }
                }
            }

            return false;
        }

        public override void Notify_Equipped(Pawn pawn)
		{
            base.Notify_Equipped(pawn);
            AthenaCache.AddCache(this, ref AthenaCache.bodyCache, pawn.thingIDNumber);
            if (pawn.Drawer.renderer.renderTree.rootNode != null)
            {
                pawn.Drawer.renderer.renderTree.rootNode.requestRecache = true;
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            AthenaCache.RemoveCache(this, AthenaCache.bodyCache, pawn.thingIDNumber);
            if (pawn.Drawer.renderer.renderTree.rootNode != null)
            {
                pawn.Drawer.renderer.renderTree.rootNode.requestRecache = true;
            }
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
                if (Wearer.Drawer.renderer.renderTree.rootNode != null)
                {
                    Wearer.Drawer.renderer.renderTree.rootNode.requestRecache = true;
                }
            }
        }

        public virtual void FurGraphic(ref Graphic furGraphic) { }
    }

    public class CompProperties_CustomApparelBody : CompProperties
    {
        public CompProperties_CustomApparelBody()
        {
            this.compClass = typeof(Comp_CustomApparelBody);
        }

        // If bodygear should ignore bodytype in their texture paths similarly to headgear.
        public bool preventBodytype = false;
        // If set to a certain bodytype it will force that bodytype onto all apparel that user is wearing. Overrides bodytypePairs
        public BodyTypeDef forcedBodytype;
        // Same as forcedBodytype, but for the head
        public HeadTypeDef forcedHeadtype;
        // If the comp should hide the owner's body
        public bool hideBody = false;
        // If the comp should hide the owner's head
        public bool hideHead = false;
        // If the comp should hide the owner's fur
        public bool hideFur = false;
        // If the comp should hide the owner's hair
        public bool hideHair = false;
        // List of bodytype pairs that will be swapped. Allows to do stuff like making all fatties into hulks and thins into normals
        public List<BodytypeSwitch> bodytypePairs;
        // Same as bodytypePairs but for heads
        public List<HeadtypeSwitch> headtypePairs;
    }

    public struct BodytypeSwitch
    {
        // Bodytype that will be switched
        public BodyTypeDef initialBodytype;
        // What bodytype are we switching to
        public BodyTypeDef newBodytype;
    }

    public struct HeadtypeSwitch
    {
        // Headtype that will be switched
        public HeadTypeDef initialHeadtype;
        // What headtype are we switching to
        public HeadTypeDef newHeadtype;
    }
}
