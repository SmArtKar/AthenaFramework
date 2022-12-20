// RimWorld.CompAbilityEffect_GiveHediff
using RimWorld;
using Verse;

namespace MechAbility
{
	public class CompAbilityEffect_GiveHediffSeverity : CompAbilityEffect
	{
		public new CompProperties_AbilityGiveHediffSeverity Props => (CompProperties_AbilityGiveHediffSeverity)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			if (!Props.onlyApplyToSelf && Props.applyToTarget)
			{
				ApplyInner(target.Pawn, parent.pawn);
			}
			if (Props.applyToSelf || Props.onlyApplyToSelf)
			{
				ApplyInner(parent.pawn, target.Pawn);
			}
		}

		protected void ApplyInner(Pawn target, Pawn other)
		{
			if (target == null)
			{
				return;
			}
			Hediff firstHediffOfDef = target.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);
			if (Props.replaceExisting && firstHediffOfDef != null)
			{
				target.health.RemoveHediff(firstHediffOfDef);
			}
			Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, target, Props.onlyBrain ? target.health.hediffSet.GetBrain() : null);
			
			// HediffComp_Link hediffComp_Link = hediff.TryGetComp<HediffComp_Link>();
			// if (hediffComp_Link != null)
			// {
				// hediffComp_Link.other = other;
				// hediffComp_Link.drawConnection = target == parent.pawn;
			// }
			if (Props.replaceExisting || firstHediffOfDef == null)
			{
				target.health.AddHediff(hediff);
			}
			else
			{
				//firstHediffOfDef.severity += Props.severity;
				HealthUtility.AdjustSeverity(target, firstHediffOfDef.def, Props.severity);
			}
		}
	}

	public class CompProperties_AbilityGiveHediffSeverity : CompProperties_AbilityEffect
	{
		public HediffDef hediffDef;

		public bool onlyBrain;

		public bool applyToSelf;

		public bool onlyApplyToSelf;

		public bool applyToTarget = true;

		public bool replaceExisting;

		public float severity = -1f;
		
		public CompProperties_AbilityGiveHediffSeverity()
		{
			compClass = typeof(CompAbilityEffect_GiveHediffSeverity);
		}
	}
}
