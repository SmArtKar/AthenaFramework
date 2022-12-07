using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;

namespace AthenaFramework
{
    public class HediffComp_Renderable : HediffComp
    {
        private HediffCompProperties_Renderable Props => props as HediffCompProperties_Renderable;
        public Mote attachedMote;

        public virtual void DrawAt(Vector3 drawPos)
        {
            if (Props.graphicData == null)
            {
                return;
            }

            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor(); 
            Props.graphicData.Graphic.Draw(drawPos, parent.pawn.Rotation, Pawn);
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            attachedMote.Maintain();
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);
            if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(parent.pawn, Props.attachedMoteDef, new Vector3(), 1.5f);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            if (attachedMote != null && !attachedMote.Destroyed)
            {
                attachedMote.Destroy();
            }
        }
    }

    public class HediffCompProperties_Renderable : HediffCompProperties
    {
        public HediffCompProperties_Renderable()
        {
            this.compClass = typeof(HediffComp_Renderable);
        }

        public GraphicData graphicData = null;
        public ThingDef attachedMoteDef = null;
    }
}
