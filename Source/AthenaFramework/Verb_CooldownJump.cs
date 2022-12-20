using RimWorld;
using Verse;

namespace MVCF.Verbs
{
	[DefOf]
	public static class TCMechs_DefOf
	{
        public static ThingDef PawnJumper_FlambergeFlight;
	}
    public class Verb_CooldownJump : RimWorld.Verb_Jump
    {
		public int cooldown = 0;
        protected override float EffectiveRange => verbProps.range;
		
		public override bool Available()
		{
			if (Find.TickManager.TicksGame > this.cooldown)
			{
				return true;
			}
			return false;
		}
        protected override bool TryCastShot()
        {
            if (!ModLister.HasActiveModWithName("Royalty") || !ModLister.HasActiveModWithName("Biotech"))
            {
                Log.ErrorOnce(
                    "Items with jump capability are a Royalty/Biotech-specific game item. If you want to use this code please enable the DLC before calling it. See rules on the Ludeon forum for more info.",
                    550187797);
                return false;
            }
			
            var casterPawn = CasterPawn;
            var cell = currentTarget.Cell;
            var map = casterPawn.Map;
            var pawnFlyer = PawnFlyer.MakeFlyer(TCMechs_DefOf.PawnJumper_FlambergeFlight, casterPawn, cell, null, null, false);
            if (pawnFlyer != null)
            {
                GenSpawn.Spawn(pawnFlyer, cell, map);
				this.cooldown = Find.TickManager.TicksGame + 3600;	//60s cooldown
                return true;
            }
			
            return false;
        }
    }
}