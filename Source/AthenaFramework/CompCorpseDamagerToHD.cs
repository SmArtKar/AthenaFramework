using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Verse.Sound;
using Verse;

namespace AnimalBehaviours
{
    public class CompCorpseDamagerToHD : ThingComp
    {
		//mostly taken from Sarg's Helixian Slug code
        public int tickCounter = 0;
        public bool flagOnce = false;
		public bool shouldDecay;

        public CompProperties_CorpseDamagerToHD Props
        {
            get
            {
                return (CompProperties_CorpseDamagerToHD)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();

			tickCounter++;

			//Only check every 2 rare ticks (8 seconds)
			if (tickCounter > Props.tickInterval)
			{
				Pawn pawn = this.parent as Pawn;

				//Null map check
				if (pawn.Map != null)
				{
					//Check on radius
					CellRect rect = GenAdj.OccupiedRect(pawn.Position, pawn.Rotation, IntVec2.One);
					rect = rect.ExpandedBy(Props.radius);

					foreach (IntVec3 current in rect.Cells)
					{
						if (current.InBounds(pawn.Map))
						{
							HashSet<Thing> hashSet = new HashSet<Thing>(current.GetThingList(pawn.Map));
							if (hashSet != null)
							{
								foreach (Thing thingInCell in hashSet)
								{
									Corpse corpse = thingInCell as Corpse;
									//If anything in those cells was a corpse
									if (corpse != null)
									{
										
										//If it's a bug corpse and we care about bugs
										if (corpse.InnerPawn.RaceProps.FleshType == FleshTypeDefOf.Insectoid && Props.hediffBugCorpse != null) 
										{
											HealthUtility.AdjustSeverity(pawn, Props.hediffBugCorpse, Props.severityGained); 
											shouldDecay = true;
										}
										//If it's a human corpse and we care about humans
										else if (corpse.InnerPawn.def.race.Humanlike && Props.hediffHumanCorpse != null) 
										{
											HealthUtility.AdjustSeverity(pawn, Props.hediffHumanCorpse, Props.severityGained); 
											shouldDecay = true;
										}
										//If it's an animal corpse and we care about animals
										else if (corpse.InnerPawn.RaceProps.Animal && Props.hediffBugCorpse != null)
										{
											HealthUtility.AdjustSeverity(pawn, Props.hediffAnimalCorpse, Props.severityGained); 
											shouldDecay = true;
										}
										//If it's a mech corpse and we care about mechs
										else if (corpse.InnerPawn.RaceProps.FleshType == FleshTypeDefOf.Mechanoid && Props.hediffMechCorpse != null) 
										{
											HealthUtility.AdjustSeverity(pawn, Props.hediffMechCorpse, Props.severityGained); 
											shouldDecay = true;
										}
										//If the corpse reaches 0 HP, destroy it, and spawn corpse bile
										if (shouldDecay) corpse.HitPoints -= Props.decayOnHitPoints;
										shouldDecay = false;
										if (corpse.HitPoints < 0)
										{
											corpse.Destroy(DestroyMode.Vanish);
											for (int i = 0; i < 20; i++)
											{
												IntVec3 c;
												CellFinder.TryFindRandomReachableCellNear(pawn.Position, pawn.Map, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
												FilthMaker.TryMakeFilth(c, pawn.Map, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite(), 1, FilthSourceFlags.None);
												SoundDef.Named(Props.corpseSound).PlayOneShot(new TargetInfo(pawn.Position, pawn.Map, false));
											}
										}
										FilthMaker.TryMakeFilth(current, pawn.Map, ThingDefOf.Filth_CorpseBile, pawn.LabelIndefinite(), 1, FilthSourceFlags.None);
										flagOnce = true;
										break;	//exit foreach without looking at every other thing in the cell for speed
									}
								}
							}
						}
						if (flagOnce && Props.findOneCorpsePerTickInterval) { flagOnce = false; break; }
					}
				}
				tickCounter = 0;
			}
        }
    }
	public class CompProperties_CorpseDamagerToHD : CompProperties
    {
        
        //A comp class to make a creature damage nearby corpses to gain hediff severity
		//Can deal 0 damage to just be empowered by nearby corpses
		//Can get different hediffs from different fleshTypes

        public int radius = 5;
        public int tickInterval = 500;
        public int decayOnHitPoints = 1000;
		public HediffDef hediffHumanCorpse;
		public HediffDef hediffBugCorpse;
		public HediffDef hediffAnimalCorpse;
		public HediffDef hediffMechCorpse;
		public bool findOneCorpsePerTickInterval = true;
        public float severityGained = 1f;
        public string corpseSound = "";

        public CompProperties_CorpseDamagerToHD()
        {
            this.compClass = typeof(CompCorpseDamagerToHD);
        }
    }
}