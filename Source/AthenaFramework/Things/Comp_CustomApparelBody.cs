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

        public virtual Graphic getBodyGraphic
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

        public virtual Graphic getHeadGraphic
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
        public virtual bool getPreventBodytype
        {
            get
            {
                return Props.preventBodytype;
            }
        }

        public virtual BodyTypeDef getBodytype
        {
            get
            {
                if (Props.forcedBodytype == null)
                {
                    return null;
                }

                return Props.forcedBodytype;
            }
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
        // If set to a certain bodytype it will force that bodytype onto all apparel that user is wearing
        public BodyTypeDef forcedBodytype;
    }
}
