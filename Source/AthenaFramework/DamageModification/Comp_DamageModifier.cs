using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class Comp_DamageModifier : ThingComp, IDamageModifier
    {
        private CompProperties_DamageModifier Props => props as CompProperties_DamageModifier;

        public virtual float OutgoingDamageMultiplier
        {
            get
            {
                return Props.outgoingDamageMultiplier;
            }
        }

        public virtual (float, float) GetOutcomingDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator, DamageInfo? dinfo, bool projectile = false)
        {
            float modifier = 1f;
            float offset = 0f;
            List<string> excluded = new List<string>();

            for(int i = Props.outgoingModifiers.Count - 1; i >= 0; i--)
            {
                DamageModificator modGroup = Props.outgoingModifiers[i];
                (float, float) result = modGroup.GetDamageModifiers(target, ref excluded, ref excludedGlobal, instigator, dinfo, projectile);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }

        public virtual (float, float) GetIncomingDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator, DamageInfo? dinfo, bool projectile = false)
        {
            float modifier = Props.incomingDamageMultiplier;
            float offset = 0f;
            List<string> excluded = new List<string>();

            for (int i = Props.incomingModifiers.Count - 1; i >= 0; i--)
            {
                DamageModificator modGroup = Props.incomingModifiers[i];
                (float, float) result = modGroup.GetDamageModifiers(instigator, ref excluded, ref excludedGlobal, target, dinfo, projectile, true);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            if (Props.workOnEquip)
            {
                AthenaCache.AddCache(this, ref AthenaCache.damageCache, pawn.thingIDNumber);
            }

            if (Props.workOnParent && !Props.workWhenEquipped)
            {
                AthenaCache.RemoveCache(this, AthenaCache.damageCache, parent.thingIDNumber);
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (Props.workOnEquip)
            {
                AthenaCache.RemoveCache(this, AthenaCache.damageCache, pawn.thingIDNumber);
            }

            if (Props.workOnParent && !Props.workWhenEquipped)
            {
                AthenaCache.AddCache(this, ref AthenaCache.damageCache, parent.thingIDNumber);
            }
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            if (Props.workOnParent)
            {
                AthenaCache.AddCache(this, ref AthenaCache.damageCache, parent.thingIDNumber);
            }
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (Props.workOnParent)
            {
                AthenaCache.RemoveCache(this, AthenaCache.damageCache, parent.thingIDNumber);
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();

            if (Scribe.mode != LoadSaveMode.ResolvingCrossRefs)
            {
                return;
            }

            Apparel apparel = parent as Apparel;

            if (apparel != null && apparel.Wearer != null)
            {
                if (Props.workOnEquip)
                {
                    AthenaCache.AddCache(this, ref AthenaCache.damageCache, apparel.Wearer.thingIDNumber);
                }

                if (Props.workOnParent && !Props.workWhenEquipped)
                {
                    AthenaCache.RemoveCache(this, AthenaCache.damageCache, parent.thingIDNumber);
                }

                return;
            }

            CompEquippable comp = parent.GetComp<CompEquippable>();

            if (comp != null && comp.Holder != null)
            {
                if (Props.workOnEquip)
                {
                    AthenaCache.AddCache(this, ref AthenaCache.damageCache, comp.Holder.thingIDNumber);
                }

                if (Props.workOnParent && !Props.workWhenEquipped)
                {
                    AthenaCache.RemoveCache(this, AthenaCache.damageCache, parent.thingIDNumber);
                }
            }
        }
    }

    public class CompProperties_DamageModifier : CompProperties
    {
        public CompProperties_DamageModifier()
        {
            this.compClass = typeof(Comp_DamageModifier);
        }

        // List of possible modification effects that affect outgoing damage
        public List<DamageModificator> outgoingModifiers = new List<DamageModificator>();
        // List of possible modification effects that affect outgoing damage
        public List<DamageModificator> incomingModifiers = new List<DamageModificator>();
        // Whenever the damage modification should be applied on the parent itself
        public bool workOnParent = true;
        // Whenever damage modifications should be applied on pawns that equips the parent
        public bool workOnEquip = true;
        // Whenever damage modifications should be applied on the parent when it's equipped
        public bool workWhenEquipped = false;
        // Passive outgoing damage modifier that's always applied
        public float outgoingDamageMultiplier = 1f;
        // Passive incoming damage modifier that's always applied
        public float incomingDamageMultiplier = 1f;
    }
}
