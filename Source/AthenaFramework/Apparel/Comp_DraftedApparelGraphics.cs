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
    public class Comp_DraftedApparelGraphics : ThingComp, IEquippableGraphicGiver
    {
        private CompProperties_DraftedApparelGraphics Props => props as CompProperties_DraftedApparelGraphics;
        private Apparel Apparel => parent as Apparel;
        private Pawn Pawn => Apparel.Wearer;

        private bool cachedDrafted = false;

        public List<ApparelGraphicPackage> GetAdditionalGraphics
        {
            get
            {
                return cachedDrafted ? Props.draftedGraphics : Props.undraftedGraphics;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.IsHashIntervalTick(15))
            {
                return;
            }

            if (Pawn == null || Pawn.drafter == null)
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

    public class CompProperties_DraftedApparelGraphics : CompProperties
    {
        public CompProperties_DraftedApparelGraphics()
        {
            this.compClass = typeof(Comp_DraftedApparelGraphics);
        }

        // Additiona graphics that are applied when the pawn is drafted
        public List<ApparelGraphicPackage> draftedGraphics = new List<ApparelGraphicPackage>();

        // Additiona graphics that are applied when the pawn is NOT drafted
        public List<ApparelGraphicPackage> undraftedGraphics = new List<ApparelGraphicPackage>();
    }
}
