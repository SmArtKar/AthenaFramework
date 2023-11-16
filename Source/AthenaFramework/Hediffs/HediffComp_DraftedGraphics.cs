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
    public class HediffComp_DraftedGraphics : HediffComp, IHediffGraphicGiver
    {
        private HediffCompProperties_DraftedGraphics Props => props as HediffCompProperties_DraftedGraphics;

        private bool cachedDrafted = false;

        public List<HediffGraphicPackage> GetAdditionalGraphics
        {
            get
            {
                return cachedDrafted ? Props.draftedGraphics : Props.undraftedGraphics;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (!Pawn.IsHashIntervalTick(15))
            {
                return;
            }

            if (Pawn.drafter == null)
            {
                return;
            }

            if (Pawn.drafter.Drafted)
            {
                if (!cachedDrafted)
                {
                    cachedDrafted = true;

                    if (AthenaCache.renderCache.TryGetValue(Pawn.thingIDNumber, out List<IRenderable> mods))
                    {
                        for (int i = mods.Count - 1; i >= 0; i--)
                        {
                            IRenderable renderable = mods[i];
                            renderable.RecacheGraphicData();
                        }
                    }
                }
            }
            else
            {
                if (cachedDrafted)
                {
                    cachedDrafted = false;

                    if (AthenaCache.renderCache.TryGetValue(Pawn.thingIDNumber, out List<IRenderable> mods))
                    {
                        for (int i = mods.Count - 1; i >= 0; i--)
                        {
                            IRenderable renderable = mods[i];
                            renderable.RecacheGraphicData();
                        }
                    }
                }
            }
        }
    }

    public class HediffCompProperties_DraftedGraphics : HediffCompProperties
    {
        public HediffCompProperties_DraftedGraphics()
        {
            this.compClass = typeof(HediffComp_DraftedGraphics);
        }

        // Additiona graphics that are applied when the pawn is drafted
        public List<HediffGraphicPackage> draftedGraphics = new List<HediffGraphicPackage>();

        // Additiona graphics that are applied when the pawn is NOT drafted
        public List<HediffGraphicPackage> undraftedGraphics = new List<HediffGraphicPackage>();
    }
}
