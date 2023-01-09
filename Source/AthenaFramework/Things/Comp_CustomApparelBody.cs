using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Comp_CustomApparelBody : ThingComp
    {
        private CompProperties_CustomApparelBody Props => props as CompProperties_CustomApparelBody;
        protected Apparel Apparel => parent as Apparel;
        protected Pawn Wearer => Apparel.Wearer;

        public virtual Graphic GetBodyGraphic
        {
            get
            {
                if (Props.bodyGraphicData == null)
                {
                    return null;
                }

                return Props.bodyGraphicData.Graphic;
            }
        }

        public virtual Graphic GetHeadGraphic
        {
            get
            {
                if (Props.headGraphicData == null)
                {
                    return null;
                }

                return Props.headGraphicData.Graphic;
            }
        }
        public virtual bool PreventBodytype (BodyTypeDef bodyType, ApparelGraphicRecord rec)
        {
            return Props.preventBodytype;
        }

        public virtual BodyTypeDef CustomBodytype(Apparel apparel, BodyTypeDef bodyType)
        {
            if (Props.forcedBodytype != null)
            {
                return Props.forcedBodytype;
            }

            if (Props.bodytypePairs != null)
            {
                for (int i = Props.bodytypePairs.Count - 1; i >= 0; i--)
                {
                    BodytypeSwitch pair = Props.bodytypePairs[i];
                    if (pair.initialBodytype == bodyType)
                    {
                        return pair.newBodytype;
                    }
                }
            }

            return null;
        }

        public virtual BodyTypeDef CustomBodytype(Apparel apparel, BodyTypeDef bodyType, ApparelGraphicRecord rec)
        {
            return CustomBodytype(apparel, bodyType);
        }

        public override void Notify_Equipped(Pawn pawn)
		{
            base.Notify_Equipped(pawn);
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }
    }

    public class CompProperties_CustomApparelBody : CompProperties
    {
        public CompProperties_CustomApparelBody()
        {
            this.compClass = typeof(Comp_CustomApparelBody);
        }

        // Custom graphic for the wearer's head
        public GraphicData bodyGraphicData = null;
        // Custom graphic for the wearer's head
        public GraphicData headGraphicData = null;
        // If bodygear should ignore bodytype in their texture paths similarly to headgear.
        public bool preventBodytype = true;
        // If set to a certain bodytype it will force that bodytype onto all apparel that user is wearing. Overrides bodytypePairs
        public BodyTypeDef forcedBodytype;
        // List of bodytype pairs that will be swapped. Allows to do stuff like making all fatties into hulks and thins into normals
        public List<BodytypeSwitch> bodytypePairs;
    }

    public class BodytypeSwitch
    {
        // Bodytype that will be switched
        public BodyTypeDef initialBodytype;
        // What bodytype are we switching from
        public BodyTypeDef newBodytype;
    }
}
