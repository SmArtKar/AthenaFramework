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
        public CompProperties_CustomApparelBody Props => props as CompProperties_CustomApparelBody;

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

        public GraphicData bodyGraphicData = null;
        public GraphicData headGraphicData = null;
    }
}
