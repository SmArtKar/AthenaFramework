using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_CompApplicator : HediffComp
    {
        private HediffCompProperties_CompApplicator Props => props as HediffCompProperties_CompApplicator;

        public List<ThingComp> linkedComps = new List<ThingComp>();

        public override void CompPostMake()
        {
            base.CompPostMake();

            for (int i = Props.comps.Count - 1; i >= 0; i--)
            {
                ThingComp comp = null;
                try
                {
                    comp = (ThingComp)Activator.CreateInstance(Props.comps[i].compClass);
                    comp.props = Props.comps[i];
                    comp.parent = Pawn;
                    Pawn.comps.Add(comp);
                    linkedComps.Add(comp);
                }
                catch (Exception ex)
                {
                    Log.Error("Hediff CompApplicator could not instantiate or initialize a ThingComp: " + ex);
                    Pawn.comps.Remove(comp);
                }
            }
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                return;
            }

            for (int i = Props.comps.Count - 1; i >= 0; i--)
            {
                ThingComp comp = null;
                try
                {
                    comp = (ThingComp)Activator.CreateInstance(Props.comps[i].compClass);
                    comp.props = Props.comps[i];
                    comp.parent = Pawn;
                    Pawn.comps.Add(comp);
                    linkedComps.Add(comp);
                }
                catch (Exception ex)
                {
                    Log.Error("Hediff CompApplicator could not instantiate or initialize a ThingComp: " + ex);
                    Pawn.comps.Remove(comp);
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            for (int i = linkedComps.Count - 1; i >= 0; i--)
            {
                Pawn.comps.Remove(linkedComps[i]);
                linkedComps.RemoveAt(i);
            }
        }
    }

    public class HediffCompProperties_CompApplicator : HediffCompProperties
    {
        public HediffCompProperties_CompApplicator()
        {
            this.compClass = typeof(HediffComp_CompApplicator);
        }

        // Comps that will be applied to the hediff owner
        public List<CompProperties> comps = new List<CompProperties>();
    }
}
