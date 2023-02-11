using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace AthenaFramework
{
    public class JobGiver_ReloadAbility : ThinkNode_JobGiver
    {
        public override float GetPriority(Pawn pawn)
        {
            return 5.9f;
        }

        public override Job TryGiveJob(Pawn pawn)
        {
            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) || pawn.abilities == null)
            {
                return null;
            }

            CompAbility_Reloadable comp = null;
            List<Thing> ammo = null;

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

                    List<Thing> compAmmo = reloadComp.FindAmmo(false);

                    if (!compAmmo.NullOrEmpty())
                    {
                        comp = reloadComp;
                        ammo = compAmmo;
                    }
                }

                if (comp != null)
                {
                    break;
                }
            }

            if (comp == null)
            {
                return null;
            }

            return MakeReloadJob(comp, ammo);
        }

        public static Job MakeReloadJob(CompAbility_Reloadable comp, List<Thing> chosenAmmo)
        {
            Job job = JobMaker.MakeJob(AthenaDefOf.Athena_ReloadAbility);
            job.targetQueueA = chosenAmmo.Select((Thing ammo) => new LocalTargetInfo(ammo)).ToList();
            job.count = chosenAmmo.Sum((Thing ammo) => ammo.stackCount);
            job.count = Math.Min(job.count, comp.MaxRequiredAmmo);
            return job;
        }
    }
}
