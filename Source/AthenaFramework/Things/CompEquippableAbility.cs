using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompEquippableAbility : ThingComp
    {
        private CompProperties_EquippableAbility Props => props as CompProperties_EquippableAbility;

        public int lastAbilityCastTick = -99999999;
        public bool addedAbility = false;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (pawn.abilities.GetAbility(Props.ability) != null)
            {
                addedAbility = false;
                return;
            }

            addedAbility = true;
            pawn.abilities.GainAbility(Props.ability);
            Ability ability = pawn.abilities.GetAbility(Props.ability);
            ability.lastCastTick = lastAbilityCastTick;
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (!addedAbility)
            {
                return;
            }

            Ability ability = pawn.abilities.GetAbility(Props.ability);
            lastAbilityCastTick = ability.lastCastTick;
            pawn.abilities.RemoveAbility(Props.ability);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref lastAbilityCastTick, "lastAbilityCastTick");
            Scribe_Values.Look(ref addedAbility, "addedAbility");

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
            {
                return;
            }

            Apparel apparel = parent as Apparel;

            if (apparel != null && apparel.Wearer != null)
            {
                if (apparel.Wearer.abilities.GetAbility(Props.ability) != null)
                {
                    addedAbility = false;
                    return;
                }

                addedAbility = true;
                apparel.Wearer.abilities.GainAbility(Props.ability);
                Ability ability = apparel.Wearer.abilities.GetAbility(Props.ability);
                ability.lastCastTick = lastAbilityCastTick;

                return;
            }

            CompEquippable comp = parent.GetComp<CompEquippable>();

            if (comp != null && comp.Holder != null)
            {
                if (apparel.Wearer.abilities.GetAbility(Props.ability) != null)
                {
                    addedAbility = false;
                    return;
                }

                addedAbility = true;
                apparel.Wearer.abilities.GainAbility(Props.ability);
                Ability ability = apparel.Wearer.abilities.GetAbility(Props.ability);
                ability.lastCastTick = lastAbilityCastTick;
            }
        }
    }

    public class CompProperties_EquippableAbility : CompProperties
    {
        public CompProperties_EquippableAbility()
        {
            this.compClass = typeof(CompEquippableAbility);
        }

        public AbilityDef ability;
    }
}
