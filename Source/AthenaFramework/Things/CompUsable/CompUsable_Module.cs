using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using static RimWorld.RitualRoleAssignments;

namespace AthenaFramework
{ 
    public class CompUsable_Module : CompUsable
    {
        public override AcceptanceReport CanBeUsedBy(Pawn p, bool forced = false, bool ignoreReserveAndReachable = false)
        {
            AcceptanceReport result = base.CanBeUsedBy(p, forced, ignoreReserveAndReachable);
            if (!result.Accepted)
            {
                return result;
            }

            CompUseEffect_Module parentComp = parent.TryGetComp<CompUseEffect_Module>();

            for (int i = p.equipment.equipment.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = p.equipment.equipment[i];

                for (int j = thing.AllComps.Count - 1; j >= 0; j--)
                {
                    CompModular comp = thing.AllComps[j] as CompModular;

                    if (comp != null && comp.GetOpenSlots(parentComp).Count > 0)
                    {
                        return true;
                    }
                }
            }

            List<Apparel> wornApparel = p.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = wornApparel[i];

                for (int j = thing.AllComps.Count - 1; j >= 0; j--)
                {
                    CompModular comp = thing.AllComps[j] as CompModular;
                    if (comp != null && comp.GetOpenSlots(parentComp).Count > 0)
                    {
                        return true;
                    }
                }
            }

            return "Cannot apply: No compatible slots availible.";
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            string label = "Install {0}".Formatted(parent.Label);

            AcceptanceReport report = CanBeUsedBy(pawn, true, Props.ignoreOtherReservations);
            if (!report.Accepted)
            {
                yield return new FloatMenuOption(label + ((report.Reason != null) ? (" (" + report.Reason + ")") : ""), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            if (!pawn.CanReach(parent, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
            {
                yield return new FloatMenuOption(label + " (" + "NoPath".Translate() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            if (!pawn.CanReserve(parent, 1, -1, null, Props.ignoreOtherReservations))
            {
                Pawn reservedPawn = pawn.Map.reservationManager.FirstRespectedReserver(parent, pawn, null) ?? pawn.Map.physicalInteractionReservationManager.FirstReserverOf(parent);
                string newLabel = label;

                if (reservedPawn != null)
                {
                    newLabel += " (" + "ReservedBy".Translate(reservedPawn.LabelShort, reservedPawn) + ")";
                }
                else
                {
                    newLabel += " (" + "Reserved".Translate() + ")";
                }

                yield return new FloatMenuOption(newLabel, null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
            {
                yield return new FloatMenuOption(label + " (" + "Incapable".Translate().CapitalizeFirst() + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            if (Props.userMustHaveHediff != null && !pawn.health.hediffSet.HasHediff(Props.userMustHaveHediff, false))
            {
                yield return new FloatMenuOption(label + " (" + "MustHaveHediff".Translate(Props.userMustHaveHediff) + ")", null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
                yield break;
            }

            CompUseEffect_Module parentComp = parent.TryGetComp<CompUseEffect_Module>();
            List<CompModular> workingModulars = new List<CompModular>();

            for (int i = pawn.equipment.equipment.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = pawn.equipment.equipment[i];

                for (int j = thing.AllComps.Count - 1; j >= 0; j--)
                {
                    CompModular comp = thing.AllComps[j] as CompModular;

                    if (comp == null)
                    {
                        continue;
                    }

                    List<ModuleSlotPackage> slots = comp.GetOpenSlots(parentComp);

                    for (int k = slots.Count - 1; k >= 0; k--)
                    {
                        ModuleSlotPackage slot = slots[k];
                        Action action = delegate ()
                        {
                            int m = k;
                            if (pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, Props.ignoreOtherReservations))
                            {
                                StartModuleJob(pawn, comp, parentComp, slots[m], Props.ignoreOtherReservations);
                            }
                        };

                        FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Install {0} in {1} ({2})".Formatted(parent.Label, thing.Label, slot.slotName), action, Icon, IconColor, Props.floatMenuOptionPriority, null, null, 0f, null, null, true, 0, HorizontalJustification.Left, false), pawn, parent, "ReservedBy", null);
                        yield return floatMenuOption;
                    }
                }
            }

            List<Apparel> wornApparel = pawn.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = wornApparel[i];

                for (int j = thing.AllComps.Count - 1; j >= 0; j--)
                {
                    CompModular comp = thing.AllComps[j] as CompModular;

                    if (comp == null)
                    {
                        continue;
                    }

                    List<ModuleSlotPackage> slots = comp.GetOpenSlots(parentComp);

                    for (int k = slots.Count - 1; k >= 0; k--)
                    {
                        ModuleSlotPackage slot = slots[k];
                        Action action = delegate ()
                        {
                            int m = k;
                            if (pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, Props.ignoreOtherReservations))
                            {
                                StartModuleJob(pawn, comp, parentComp, slots[m], Props.ignoreOtherReservations);
                            }
                        };

                        FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Install {0} in {1} ({2})".Formatted(parent.Label, thing.Label, slot.slotName), action, Icon, IconColor, Props.floatMenuOptionPriority, null, null, 0f, null, null, true, 0, HorizontalJustification.Left, false), pawn, parent, "ReservedBy", null);
                        yield return floatMenuOption;
                    }
                }
            }

            yield break;
        }

        public virtual void StartModuleJob(Pawn pawn, CompModular modular, CompUseEffect_Module parentComp, ModuleSlotPackage slot, bool forced = false)
        {
            parentComp.ownerThing = modular.parent;
            parentComp.usedSlot = slot.slotID;
            TryStartUseJob(pawn, null, forced);
        }

    }
}
