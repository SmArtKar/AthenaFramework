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
    public class CompUsable_HediffModule : CompUsable
    {
        public override bool CanBeUsedBy(Pawn pawn, out string failReason)
        {
            if (!base.CanBeUsedBy(pawn, out failReason))
            {
                return false;
            }

            CompUseEffect_HediffModule parentComp = parent.TryGetComp<CompUseEffect_HediffModule>();

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps hediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                if (hediff == null)
                {
                    continue;
                }

                for (int j = hediff.comps.Count - 1; j >= 0; j--)
                {
                    HediffComp_Modular comp = hediff.comps[j] as HediffComp_Modular;

                    if (comp != null && comp.GetOpenSlots(parentComp).Count > 0)
                    {
                        failReason = null;
                        return true;
                    }
                }
            }

            failReason = "Cannot apply: No compatible slots availible.";
            return false;
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            string text;
            string label = "Install {0}".Formatted(parent.Label);

            if (!CanBeUsedBy(pawn, out text))
            {
                yield return new FloatMenuOption(label + ((text != null) ? (" (" + text + ")") : ""), null, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0);
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

            CompUseEffect_HediffModule parentComp = parent.TryGetComp<CompUseEffect_HediffModule>();
            List<HediffComp_Modular> workingModulars = new List<HediffComp_Modular>();

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps hediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                if (hediff == null)
                {
                    continue;
                }

                for (int j = hediff.comps.Count - 1; j >= 0; j--)
                {
                    HediffComp_Modular comp = hediff.comps[j] as HediffComp_Modular;

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
                            if (pawn.CanReserveAndReach(parent, PathEndMode.Touch, Danger.Deadly, 1, -1, null, Props.ignoreOtherReservations))
                            {
                                StartModuleJob(pawn, comp, parentComp, slot, Props.ignoreOtherReservations);
                            }
                        };

                        string labeled = "Install {0} in {1} ({2})".Formatted(parent.Label, hediff.Label, slots[k].slotName);

                        if (hediff.Part != null && hediff.Part.Label != null)
                        {
                            labeled = "Install {0} in {1} ({2}, {3})".Formatted(parent.Label, hediff.Label, hediff.Part.Label, slots[k].slotName);
                        }

                        FloatMenuOption floatMenuOption = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(labeled, action, Icon, IconColor, Props.floatMenuOptionPriority, null, null, 0f, null, null, true, 0, HorizontalJustification.Left, false), pawn, parent, "ReservedBy", null);
                        yield return floatMenuOption;
                    }
                }
            }

            yield break;
        }

        public virtual void StartModuleJob(Pawn pawn, HediffComp_Modular modular, CompUseEffect_HediffModule parentComp, ModuleSlotPackage slot, bool forced = false)
        {
            parentComp.ownerHediff = modular.parent;
            parentComp.usedSlot = slot.slotID;
            TryStartUseJob(pawn, null, forced);
        }
    }
}
