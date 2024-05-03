using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Runtime.Remoting.Messaging;
using Verse.Sound;
using Verse.AI;
using RimWorld.Utility;

namespace AthenaFramework
{
    public class CompAbility_Reloadable : CompAbilityEffect, IFloatMenu
    {
        private new CompProperties_AbilityReloadable Props => props as CompProperties_AbilityReloadable;
        private Pawn Pawn => parent.pawn;

        public int remainingCharges = -1;

        #region ===== Properties =====

        public virtual int MaxCharges
        {
            get
            {
                return Props.maxCharges;
            }
        }

        public virtual string LabelRemaining
        {
            get
            {
                return $"{remainingCharges} / {MaxCharges}";
            }
        }

        public virtual ThingDef AmmoDef
        {
            get
            {
                return Props.ammoDef;
            }
        }

        public virtual int AmmoPerCharge
        {
            get
            {
                return Props.ammoPerCharge;
            }
        }

        public virtual int MaxRequiredAmmo
        {
            get
            {
                return (MaxCharges - remainingCharges) * AmmoPerCharge;
            }
        }

        public virtual int ReloadDuration
        {
            get
            {
                return Props.reloadDuration;
            }
        }

        #endregion

        public override void Initialize(AbilityCompProperties props)
        {
            base.Initialize(props);
            AthenaCache.AddCache(this, ref AthenaCache.menuCache, Pawn.thingIDNumber);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref remainingCharges, "remainingCharges");
        }

        public virtual void Created()
        {
            remainingCharges = Props.maxCharges;
        }

        public override bool GizmoDisabled(out string reason)
        {
            if (remainingCharges == 0)
            {
                reason = Props.noChargesRemaining;
                return true;
            }

            reason = null;
            return false;
        }

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            remainingCharges--;

            if (Props.removeOnceEmpty)
            {
                Pawn.abilities.abilities.Remove(parent);
            }
        }

        public override string CompInspectStringExtra()
        {
            return "Remaining charges: " + LabelRemaining;
        }

        public virtual bool ShouldReload(bool forceReload = false)
        {
            if (AmmoDef == null || remainingCharges == MaxCharges)
            {
                return false;
            }

            if (!forceReload)
            {
                return remainingCharges == 0;
            }

            return true;
        }

        public virtual bool NeedsReload(Thing ammo, bool forceReload = false)
        {
            if (!ShouldReload(forceReload))
            {
                return false;
            }

            if (ammo.def != AmmoDef || ammo.stackCount < AmmoPerCharge)
            {
                return false;
            }

            return true;
        }

        public virtual void Reload(Thing ammo)
        {
            if (!NeedsReload(ammo, true))
            {
                return;
            }

            int chargesRestored = Math.Min(MaxCharges - remainingCharges, (int)(ammo.stackCount / AmmoPerCharge));
            ammo.SplitOff(chargesRestored * AmmoPerCharge).Destroy();
            remainingCharges += chargesRestored;

            if (Props.reloadSound != null)
            {
                Props.reloadSound.PlayOneShot(new TargetInfo(Pawn.Position, Pawn.Map));
            }
        }

        public virtual List<Thing> FindAmmo(bool forceReload)
        {
            if (!ShouldReload(forceReload))
            {
                return null;
            }

            IntRange amount = new IntRange(AmmoPerCharge, MaxRequiredAmmo);
            return RefuelWorkGiverUtility.FindEnoughReservableThings(Pawn, Pawn.Position, amount, (Thing ammo) => ammo.def == AmmoDef);
        }

        public virtual IEnumerable<FloatMenuOption> ItemFloatMenuOptions(Pawn selPawn) { yield break; }

        public virtual IEnumerable<FloatMenuOption> PawnFloatMenuOptions(ThingWithComps thing)
        {
            if (thing.IsForbidden(Pawn) || !Pawn.CanReserve(thing) || thing.def != AmmoDef)
            {
                yield break;
            }

            string reloadText = "Reload " + parent.def.LabelCap + " with " + thing.LabelCap + " (" + LabelRemaining + ")";

            if (!Pawn.CanReach(thing, PathEndMode.ClosestTouch, Danger.Deadly))
            {
                yield return new FloatMenuOption(reloadText + ": " + "NoPath".Translate().CapitalizeFirst(), null);
                yield break;
            }

            if (!ShouldReload(true))
            {
                yield return new FloatMenuOption(reloadText + ": " + "ReloadFull".Translate(), null);
                yield break;
            }

            if (thing.stackCount < AmmoPerCharge)
            {
                yield return new FloatMenuOption(reloadText + ": " + "ReloadNotEnough".Translate(), null);
                yield break;
            }

            // The hell is this code

            /*

            if (!NeedsReload(thing, true))
            {
                yield return new FloatMenuOption(reloadText + ": " + "ReloadFull".Translate(), null);
                yield break;
            }

            List<Thing> ammo = FindAmmo(true);

            if (ammo.NullOrEmpty())
            {
                yield return new FloatMenuOption(reloadText + ": " + "ReloadNotEnough".Translate(), null);
                yield break;
            }

            if (Pawn.carryTracker.AvailableStackSpace(AmmoDef) < AmmoPerCharge)
            {
                yield return new FloatMenuOption(reloadText + ": " + "ReloadCannotCarryEnough".Translate(NamedArgumentUtility.Named(AmmoDef, "AMMO")), null);
                yield break;
            }

            */

            Action action = delegate
            {
                Pawn.jobs.TryTakeOrderedJob(JobGiver_ReloadAbility.MakeReloadJob(this, new List<Thing>() { thing }), JobTag.Misc);
            };

            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(reloadText, action), Pawn, thing);
            yield break;
        }
    }

    public class CompProperties_AbilityReloadable : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityReloadable()
        {
            compClass = typeof(CompAbility_Reloadable);
        }

        // Maximum amount of charges stored
        public int maxCharges = 1;
        // Amount of ammo spent per charge
        public int ammoPerCharge = 1;
        // Whenever the ability is removed upon all charges being spent
        public bool removeOnceEmpty = false;
        // ThingDef of an item that refills this ability
        public ThingDef ammoDef;
        // How long it takes to reload
        public int reloadDuration = 60;
        // Sound that's played upon reloading
        public SoundDef reloadSound;
        // String that's displayed when no charges are remaining
        public string noChargesRemaining = "No charges remaining.";
    }
}
