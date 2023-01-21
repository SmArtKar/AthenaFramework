using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
using System.Reflection;

namespace AthenaFramework
{
    public class HediffComp_Renderable : HediffComp, IRenderable
    {
        private HediffCompProperties_Renderable Props => props as HediffCompProperties_Renderable;
        private static readonly float altitude = AltitudeLayer.MoteOverhead.AltitudeFor();

        public Mote attachedMote;

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
            if (Props.onlyRenderWhenDrafted && (Pawn.drafter == null || !Pawn.drafter.Drafted))
            {
                return;
            }

            if (Props.graphicData == null)
            {
                return;
            }

            Props.graphicData.Graphic.Draw(new Vector3(drawPos.x, altitude, drawPos.z), Pawn.Rotation, Pawn);

            for (int i = Props.additionalGraphics.Count - 1; i>= 0; i--)
            {
                HediffGraphicPackage package = Props.additionalGraphics[i];
                Vector3 offset = new Vector3();

                if (package.onlyRenderWhenDrafted && (Pawn.drafter == null || !Pawn.drafter.Drafted))
                {
                    return;
                }

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

                package.GetGraphic(parent).Draw(drawPos + offset, Pawn.Rotation, Pawn);
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            
            if (attachedMote != null && !attachedMote.Destroyed)
            {
                attachedMote.Maintain();
            }

            else if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
            }
        }

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
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

        // Displayed graphic. This graphic is always drawn above the pawn at MoteOverhead altitude layer
        public GraphicData graphicData;
        // Mote attached to the hediff
        public ThingDef attachedMoteDef;
        // Offset of the attached mote
        public Vector3 attachedMoteOffset = new Vector3();
        // Scale of the attached mote
        public float attachedMoteScale = 1f;
        // If set to true, attached mote will be destroyed after hediff's removal
        public bool destroyMoteOnRemoval = true;
        // If this graphic should be rendered only when the pawn is drafted. Overriden by graphic package settings
        public bool onlyRenderWhenDrafted = false;
        // Additional graphic layers with more precise controls. These are drawn on the same layer as pawn by default
        public List<HediffGraphicPackage> additionalGraphics = new List<HediffGraphicPackage>();
    }
}
