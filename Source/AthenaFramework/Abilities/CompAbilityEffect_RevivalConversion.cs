using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class CompAbilityEffect_RevivalConversion : CompAbilityEffect
    {
        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            Corpse corpse = target.Thing as Corpse;
            if (corpse == null)
            {
                return;
            }

            Pawn pawn = corpse.InnerPawn;
            if (pawn == null)
            {
                return;
            }

            ResurrectionUtility.Resurrect(pawn);
            pawn.SetFaction(parent.pawn.Faction, null);

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
            
            Corpse corpse = target.Thing as Corpse;
            if (corpse == null)
            {
                return false;
            }

            Pawn pawn = corpse.InnerPawn;
            return pawn != null && pawn.Dead && AbilityUtility.ValidateMustBeHuman(pawn, throwMessages, this.parent);
        }

        public override bool AICanTargetNow(LocalTargetInfo target)
        {
            if (!base.AICanTargetNow(target))
            {
                return false;
            }

            Corpse corpse = target.Thing as Corpse;
            if (corpse == null)
            {
                return false;
            }

            Pawn pawn = corpse.InnerPawn;
            return pawn != null && pawn.Dead && AbilityUtility.ValidateMustBeHuman(pawn, false, this.parent) && pawn.Faction.HostileTo(this.parent.pawn.Faction);
        }
    }

    public class CompProperties_RevivalConversion : CompProperties_AbilityEffect
    {
        public CompProperties_RevivalConversion()
        {
            compClass = typeof(CompAbilityEffect_RevivalConversion);
        }
    }
}
