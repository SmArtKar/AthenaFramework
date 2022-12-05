using Mono.Unix.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Comp_HediffBodypartGiver : ThingComp
    {
        private CompProperties_HediffBodypartGiver Props => props as CompProperties_HediffBodypartGiver;

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.Spawned)
            {
                return;
            }

            if (!(parent is Pawn))
            {
                Log.Error(String.Format("Comp_HediffBodypartGiver spawned on a non-pawn Thing {0}", parent.def.defName));
                parent.AllComps.Remove(this);
                return;
            }
            Pawn pawn = parent as Pawn;
            foreach (HediffBodypartPair pair in Props.bodypartPairs)
            {
                List<BodyPartRecord> partRecords = pawn.RaceProps.body.GetPartsWithDef(pair.bodyPartDef);
                foreach (BodyPartRecord partRecord in partRecords)
                {
                    if (!pawn.health.hediffSet.HasHediff(pair.hediffDef, partRecord))
                    {
                        Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, pawn, partRecord);
                        pawn.health.AddHediff(hediff, partRecord, null, null);
                    }
                }
            }
            
            parent.AllComps.Remove(this);
        }
    }

    public class CompProperties_HediffBodypartGiver : CompProperties
    {
        public CompProperties_HediffBodypartGiver()
        {
            compClass = typeof(Comp_HediffBodypartGiver);
        }

        public List<HediffBodypartPair> bodypartPairs;
    }

    public class HediffBodypartPair
    {
        public BodyPartDef bodyPartDef;
        public HediffDef hediffDef;
    }
}
