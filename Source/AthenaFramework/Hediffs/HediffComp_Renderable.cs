using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace AthenaFramework
{
    public class HediffComp_Renderable : HediffComp
    {
        private HediffCompProperties_Renderable Props => props as HediffCompProperties_Renderable;

        public virtual void DrawAt(Vector3 drawPos)
        {
            if (Props.graphicData == null)
            {
                return;
            }

            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor(); 
            Props.graphicData.Graphic.Draw(drawPos, parent.pawn.Rotation, Pawn);
        }
    }

    public class HediffCompProperties_Renderable : HediffCompProperties
    {
        public HediffCompProperties_Renderable()
        {
            this.compClass = typeof(HediffComp_Renderable);
        }

        public GraphicData graphicData = null;
    }
}
