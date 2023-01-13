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
    public class CompAbilityEffect_RevivalConversion : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Thing thing = target.Thing;

            if (thing is not Corpse)
            {
                return;
            }

            Pawn pawn = ((Corpse)thing).InnerPawn;

            if (pawn == null)
            {
                return;
            }

            ResurrectionUtility.Resurrect(pawn);
            pawn.SetFaction(parent.pawn.Faction);

            if (ModLister.CheckIdeology("Ideoligion conversion"))
            {
                pawn.ideo.SetIdeo(parent.pawn.Ideo);
            }

            if (!ModLister.CheckBiotech("xenogerm reimplantation"))
            {
                return;
            }

            GeneUtility.ReimplantXenogerm(parent.pawn, pawn);
            FleckMaker.AttachedOverlay(pawn, FleckDefOf.FlashHollow, new Vector3(0f, 0f, 0.26f), 1f, -1f);
            if (PawnUtility.ShouldSendNotificationAbout(parent.pawn) || PawnUtility.ShouldSendNotificationAbout(pawn))
            {
                int max = HediffDefOf.XenogerminationComa.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max;
                int max2 = HediffDefOf.XenogermLossShock.CompProps<HediffCompProperties_Disappears>().disappearsAfterTicks.max;
                Find.LetterStack.ReceiveLetter("LetterLabelGenesImplanted".Translate(), "LetterTextGenesImplanted".Translate(parent.pawn.Named("CASTER"), pawn.Named("TARGET"), max.ToStringTicksToPeriod(true, false, true, true, false).Named("COMADURATION"), max2.ToStringTicksToPeriod(true, false, true, true, false).Named("SHOCKDURATION")), LetterDefOf.NeutralEvent, new LookTargets(new TargetInfo[]
                {
                    parent.pawn,
                    pawn
                }), null, null, null, null);
            }
        }
        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return this.Valid(target, false);
        }

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            if (!base.Valid(target, throwMessages))
            {
                return false;
            }

            Thing thing = target.Thing;
            if (thing is not Corpse)
            {
                return false;
            }

            Pawn pawn = ((Corpse)thing).InnerPawn;

            return pawn != null && AbilityUtility.ValidateMustBeHuman(pawn, throwMessages, parent) && pawn.Dead;
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            Thing thing = target.Thing;
            if (thing is not Corpse)
            {
                return false;
            }

            Pawn pawn = ((Corpse)thing).InnerPawn;

            return pawn != null && AbilityUtility.ValidateMustBeHuman(pawn, false, parent) && pawn.Dead && pawn.Faction.HostileTo(parent.pawn.Faction);
        }
    }

    public class CompProperties_AbilityRevivalConversion : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityRevivalConversion()
        {
            compClass = typeof(CompAbilityEffect_RevivalConversion);
        }
    }
}
