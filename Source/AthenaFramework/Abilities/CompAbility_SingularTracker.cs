using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AthenaFramework
{
    public class CompAbility_SingularTracker : CompAbilityEffect
    {
        public int abilityCount = 0;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref abilityCount, "abilityCount");
        }

        public virtual void AddAbility()
        {
            abilityCount++;
        }

        public virtual void RemoveAbility()
        {
            if (abilityCount > 0)
            {
                parent.pawn.abilities.abilities.Remove(parent);
                abilityCount--;
            }
        }
    }

    public class CompProperties_AbilitySingularTracker : CompProperties_AbilityEffect
    {
        public CompProperties_AbilitySingularTracker()
        {
            compClass = typeof(CompAbility_SingularTracker);
        }
    }
}
