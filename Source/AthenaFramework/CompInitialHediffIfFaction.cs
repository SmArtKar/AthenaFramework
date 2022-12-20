using Verse;

namespace AnimalBehaviours
{
    public class CompInitialHediffIfFaction : ThingComp
    {
        private bool addHediffOnce = true;
     
        public CompProperties_InitialHediffIfFaction Props
        {
            get
            {
                return (CompProperties_InitialHediffIfFaction)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.addHediffOnce, "addHediffOnce", true, false);
        }

        public override void CompTickRare()
        {

            base.CompTickRare();

            //addHediffOnce is used (and saved) so the hediff is only added once when the creature spawns
            if (addHediffOnce)
            {
                Pawn pawn = this.parent as Pawn;
				if (Props.factionName == "" || pawn.Faction.def.defName == Props.factionName)
				{
					pawn.health.AddHediff(HediffDef.Named(Props.hediffName));               
				}
				addHediffOnce = false;
            }
        }
    }
	
	public class CompProperties_InitialHediffIfFaction : CompProperties
    {

        //A comp class that makes animals always spawn with an initial Hediff if part of a faction

        public string hediffName = "";
		public string factionName;
        public float hediffSeverity = 1f;

        public CompProperties_InitialHediffIfFaction()
        {
            this.compClass = typeof(CompInitialHediffIfFaction);
        }
    }
}
