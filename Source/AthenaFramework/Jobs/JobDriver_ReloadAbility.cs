using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using RimWorld;
using Verse;

namespace AthenaFramework
{
    public class JobDriver_ReloadAbility : JobDriver
    {
        private CompAbility_Reloadable Comp
        {
            get
            {
                for (int i = pawn.abilities.abilities.Count - 1; i >= 0; i--)
                {
                    Ability ability = pawn.abilities.abilities[i];

                    for (int j = ability.comps.Count - 1; j >= 0; j--)
                    {
                        CompAbility_Reloadable reloadComp = ability.comps[j] as CompAbility_Reloadable;

                        if (reloadComp == null)
                        {
                            continue;
                        }

                        if (pawn.carryTracker.AvailableStackSpace(reloadComp.AmmoDef) < reloadComp.AmmoPerCharge)
                        {
                            continue;
                        }

                        if (reloadComp.NeedsReload(job.targetQueueA[0].Thing, true))
                        {
                            return reloadComp;
                        }
                    }
                }

                return null;
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
            return true;
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            CompAbility_Reloadable comp = Comp;

            this.FailOn(() => comp == null);
            this.FailOn(() => comp.parent.pawn != pawn);
            this.FailOn(() => !comp.ShouldReload(true));
            this.FailOnIncapable(PawnCapacityDefOf.Manipulation);

            Toil getNextIngredient = Toils_General.Label();

            yield return getNextIngredient;
            foreach (Toil item in ReloadAsMuchAsPossible(comp))
            {
                yield return item;
            }

            yield return Toils_JobTransforms.ExtractNextTargetFromQueue(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(TargetIndex.A).FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            yield return Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true).FailOnDestroyedNullOrForbidden(TargetIndex.A);
            yield return Toils_Jump.JumpIf(getNextIngredient, () => !job.GetTargetQueue(TargetIndex.A).NullOrEmpty());

            foreach (Toil item2 in ReloadAsMuchAsPossible(comp))
            {
                yield return item2;
            }

            Toil toil = ToilMaker.MakeToil("MakeNewToils");
            toil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                if (carriedThing != null && !carriedThing.Destroyed)
                {
                    pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out var _);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
        }

        public IEnumerable<Toil> ReloadAsMuchAsPossible(CompAbility_Reloadable comp)
        {
            Toil done = Toils_General.Label();
            yield return Toils_Jump.JumpIf(done, () => pawn.carryTracker.CarriedThing == null || pawn.carryTracker.CarriedThing.stackCount < comp.AmmoPerCharge);
            yield return Toils_General.Wait(comp.ReloadDuration).WithProgressBarToilDelay(TargetIndex.A);
            Toil toil = ToilMaker.MakeToil("ReloadAsMuchAsPossible");
            toil.initAction = delegate
            {
                Thing carriedThing = pawn.carryTracker.CarriedThing;
                comp.Reload(carriedThing);
            };
            toil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return toil;
            yield return done;
        }
    }
}
