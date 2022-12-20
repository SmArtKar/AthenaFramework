using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.Sound;
using Verse.AI.Group;


namespace AnimalBehaviours
{
    public class HediffCompSummonFromHediffSeverity : HediffComp
    {
        public int tickCounter = 0;

        public HediffCompProperties_SummonFromHediffSeverity Props
        {
            get
            {
                return (HediffCompProperties_SummonFromHediffSeverity)this.props;
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            tickCounter++;
            if (tickCounter > Props.tickInterval && this.parent.Severity >= Props.severityPerSummon)
            {
                if (this.parent.pawn.Map != null)
                {
                    int numToSpawn = Rand.RangeInclusive(Props.groupMinMax[0], Props.groupMinMax[1]);
					Lord lord = ((this.parent.pawn is Pawn p) ? p.GetLord() : null);
                    for (int i = 0; i < numToSpawn; i++)
                    {
						//string factionNameToMake = Props.factionToMake;
						Faction factionToMake = this.parent.pawn.Faction; 
						if (Props.factionToMake != "")
						{
							factionToMake = Find.FactionManager.FirstFactionOfDef(FactionDef.Named(Props.factionToMake));
						}
						PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDef.Named(Props.pawnDef), factionToMake, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, 1f, false, false, true, true, false, false);
						//if (factionNameToMake == "")
						//{
							//request = new PawnGenerationRequest(PawnKindDef.Named(Props.pawnDef), this.parent.pawn.Faction, PawnGenerationContext.NonPlayer, -1, false, false, false, false, true, 1f, false, false, true, true, false, false);
							//factionNameToMake = this.parent.pawn.Faction.Name;
							//or Faction.def.defName, not sure which is which
						//}
						//have a Faction variable that contains the Props value, if empty put in FirstFactionOfDef blabla, then generate
						Pawn pawn = PawnGenerator.GeneratePawn(request);
                        GenSpawn.Spawn(pawn, CellFinder.RandomClosewalkCellNear(this.parent.pawn.Position, this.parent.pawn.Map, 2, null), this.parent.pawn.Map, WipeMode.Vanish);
                        lord?.AddPawn(pawn);
						if (Props.summonsAreManhunters)
                        {
                            pawn.mindState.mentalStateHandler.TryStartMentalState(DefDatabase<MentalStateDef>.GetNamed("ManhunterPermanent", true), null, true, false, null, false);
                        }
                    }
                    SoundDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(this.parent.pawn.Position, this.parent.pawn.Map, false));
					HealthUtility.AdjustSeverity(this.parent.pawn, this.parent.def, Props.severityPerSummon * -1);
                }
				tickCounter = 0;
            }
        }
    }
    public class HediffCompProperties_SummonFromHediffSeverity : HediffCompProperties
    {

        //A comp class to make an animal "summon" a configurable group of additional animals when spawned.
        //For example a spider spawning with a host of smaller spiders. Similar to base game wild spawns, but
        //with different defs

        public string pawnDef;						//if unstated, summons a copy of the pawn; only looks nice on nonhumans
        public List<int> groupMinMax;
        public bool summonsAreManhunters;
		public string factionToMake = "";			//if unstated, summons a pawn of the same faction as the spawner
													//Hediffs have a property called source but I don't know what it refers to, sooo
		public int tickInterval = 600;				//by default checks every ten seconds to reduce lag
		public int severityPerSummon = 1;			//need at least that much, subtract that much from severity per summon

        public HediffCompProperties_SummonFromHediffSeverity()
        {
            this.compClass = typeof(HediffCompSummonFromHediffSeverity);
        }
    }
}
