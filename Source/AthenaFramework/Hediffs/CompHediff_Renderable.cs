using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace AthenaFramework
{
    public class CompHediff_Renderable : HediffComp
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

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (!AthenaHediffUtility.renderableCompsByPawn.ContainsKey(parent.pawn))
            {
                AthenaHediffUtility.renderableCompsByPawn[parent.pawn] = new List<CompHediff_Renderable>();
            }

            AthenaHediffUtility.renderableCompsByPawn[parent.pawn].Add(this);


        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            AthenaHediffUtility.renderableCompsByPawn[parent.pawn].Remove(this);

            if (AthenaHediffUtility.renderableCompsByPawn[parent.pawn].Count == 0)
            {
                AthenaHediffUtility.renderableCompsByPawn.Remove(parent.pawn);
            }
        }
    }

    public class HediffCompProperties_Renderable : HediffCompProperties
    {
        public HediffCompProperties_Renderable()
        {
            this.compClass = typeof(CompHediff_Renderable);
        }

        public GraphicData graphicData = null;
    }
}
