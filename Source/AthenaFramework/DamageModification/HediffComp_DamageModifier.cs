﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_DamageModifier : HediffComp, IDamageModifier
    {
        private HediffCompProperties_DamageModifier Props => props as HediffCompProperties_DamageModifier;

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

            for (int i = Props.outgoingModifiers.Count - 1; i >= 0; i--)
            {
                DamageModificator modGroup = Props.outgoingModifiers[i];
                (float, float) result = modGroup.GetDamageModifiers(target, ref excluded, ref excludedGlobal, instigator, dinfo, projectile);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            if (Props.outgoingDamageCurve != null)
            {
                modifier *= Props.outgoingDamageCurve.Evaluate(parent.Severity);
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

            if (Props.incomingDamageCurve != null)
            {
                modifier *= Props.incomingDamageCurve.Evaluate(parent.Severity);
            }

            return (modifier, offset);
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            AthenaCache.AddCache(this, ref AthenaCache.damageCache, Pawn.thingIDNumber);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                AthenaCache.AddCache(this, ref AthenaCache.damageCache, Pawn.thingIDNumber);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            AthenaCache.RemoveCache(this, AthenaCache.damageCache, Pawn.thingIDNumber);
        }
    }

    public class HediffCompProperties_DamageModifier : HediffCompProperties
    {
        public HediffCompProperties_DamageModifier()
        {
            this.compClass = typeof(HediffComp_DamageModifier);
        }

        // List of possible modification effects that affect outgoing damage
        public List<DamageModificator> outgoingModifiers = new List<DamageModificator>();
        // List of possible modification effects that affect outgoing damage
        public List<DamageModificator> incomingModifiers = new List<DamageModificator>();
        // Passive outgoing damage modifier that's always applied
        public float outgoingDamageMultiplier = 1f;
        // Passive incoming damage modifier that's always applied
        public float incomingDamageMultiplier = 1f;
        // Curves for outgoing and incoming damage multiplication based on hediff's severity
        public SimpleCurve outgoingDamageCurve;
        public SimpleCurve incomingDamageCurve;
    }
}
