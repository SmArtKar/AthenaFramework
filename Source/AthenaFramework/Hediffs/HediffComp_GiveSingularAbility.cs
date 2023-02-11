using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class HediffComp_GiveSingularAbility : HediffComp
    {
        private HediffCompProperties_GiveSingularAbility Props => props as HediffCompProperties_GiveSingularAbility;

        public override void CompPostPostAdd(DamageInfo? dinfo)
        {
            base.CompPostPostAdd(dinfo);

            for (int i = Props.abilityDefs.Count; i >= 0; i--)
            {
                Ability ability = parent.pawn.abilities.GetAbility(Props.abilityDefs[i]);

                if (ability == null)
                {
                    parent.pawn.abilities.GainAbility(Props.abilityDefs[i]);
                    continue;
                }

                for (int j = ability.comps.Count - 1; j >= 0; j--)
                {
                    CompAbility_SingularTracker tracker = ability.comps[j] as CompAbility_SingularTracker;

                    if (tracker != null)
                    {
                        tracker.AddAbility();
                    }
                }
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            for (int i = 0; i < Props.abilityDefs.Count; i++)
            {
                Ability ability = parent.pawn.abilities.GetAbility(Props.abilityDefs[i]);

                if (ability == null)
                {
                    continue;
                }

                for (int j = ability.comps.Count - 1; j >= 0; j--)
                {
                    CompAbility_SingularTracker tracker = ability.comps[j] as CompAbility_SingularTracker;

                    if (tracker != null)
                    {
                        tracker.RemoveAbility();
                    }
                }
            }
        }
    }

    public class HediffCompProperties_GiveSingularAbility : HediffCompProperties
    {
        public List<AbilityDef> abilityDefs;

        public HediffCompProperties_GiveSingularAbility()
        {
            compClass = typeof(HediffComp_GiveSingularAbility);
        }
    }
}
