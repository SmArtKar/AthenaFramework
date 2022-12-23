using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace AthenaFramework
{
    public class Comp_DamageAmplifier : ThingComp
    {
        private CompProperties_DamageAmplifier Props => props as CompProperties_DamageAmplifier;

        public virtual float DamageMultiplier
        {
            get
            {
                return Props.damageMultiplier;
            }
        }

        public virtual (float, float) GetDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator = null)
        {
            float modifier = 1f;
            float offset = 0f;
            List<string> excluded = new List<string>();

            foreach(AmplificationType modGroup in Props.modifiers)
            {
                (float, float) result = modGroup.GetDamageModifiers(target, ref excluded, ref excludedGlobal, instigator);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }
    }

    public class CompProperties_DamageAmplifier: CompProperties
    {
        public CompProperties_DamageAmplifier()
        {
            this.compClass = typeof(Comp_DamageAmplifier);
        }

        // List of possible amplification effects
        public List<AmplificationType> modifiers = new List<AmplificationType>();
        // Passive damage modifier that's always applied
        public float damageMultiplier = 1f;
    }

    public class AmplificationType
    {
        // List of FleshTypeDefs that are required to trigger the effect
        public List<FleshTypeDef> fleshTypes;
        // List of PawnKinds that are required to trigger the effect
        public List<PawnKindDef> pawnKinds;
        // List of ThingDefs that are required to trigger the effect
        public List<ThingDef> thingDefs;
        // List of HediffDefs that are required to trigger the effect
        public List<HediffDef> hediffDefs;
        // Faction that's required to trigger the effect
        public FactionDef factionDef;
        // List of mutually exclusive effects. If any effect with a common element in this field has been applied, this effect won't apply
        public List<string> excluded;
        // Same as above, but applies to all damage amplification from this pawn/object, it's apparel, it's hediffs, etc. Does not intercept with excluded.
        public List<string> excludedGlobal;
        // List of target's StatDefs that multiply the damage
        public List<StatDef> targetStatDefs;
        // List of attacker's StatDefs that multiply the damage
        public List<StatDef> attackerStatDefs;
        // Number by which the damage is modified when the conditions above are met
        public float modifier = 1f;
        // Number by which the damage is passively offset when the conditions above are met. Applied after all modifiers.
        public float offset = 0f;

        public virtual (float, float) GetDamageModifiers(Thing target, ref List<string> excludedLocal, ref List<string> excludedGlobalInput, Thing instigator)
        {
            if (excluded != null)
            {
                if (excludedLocal.Intersect(excluded).ToList().Count > 0)
                {
                    return (1f, 0f);
                }
            }

            if (excludedGlobal != null)
            {
                if (excludedGlobalInput.Intersect(excludedGlobal).ToList().Count > 0)
                {
                    return (1f, 0f);
                }
            }

            if (thingDefs != null && !thingDefs.Contains(target.def))
            {
                return (1f, 0f);
            }

            if (factionDef != null && (target.Faction == null || target.Faction.def != factionDef))
            {
                return (1f, 0f);
            }

            if (fleshTypes != null || pawnKinds != null || hediffDefs != null)
            {
                if (target is not Pawn)
                {
                    return (1f, 0f);
                }

                Pawn pawn = target as Pawn;

                if (fleshTypes != null && !fleshTypes.Contains(pawn.def.race.FleshType))
                {
                    return (1f, 0f);
                }

                if (pawnKinds != null && !pawnKinds.Contains(pawn.kindDef))
                {
                    return (1f, 0f);
                }

                if (hediffDefs != null)
                {
                    List <HediffDef> localHediffDefs = new List<HediffDef>(hediffDefs);
                    foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                    {
                        if (localHediffDefs.Contains(hediff.def))
                        {
                            localHediffDefs.Remove(hediff.def);
                        }
                    }

                    if (localHediffDefs.Count > 0)
                    {
                        return (1f, 0f);
                    }
                }
            }

            if (excluded != null)
            {
                excludedLocal = excludedLocal.Concat(excluded).ToList();
            }

            if (excludedGlobal != null)
            {
                excludedGlobalInput = excludedGlobalInput.Concat(excludedGlobal).ToList();
            }

            float modifierStat = 1f;

            if (targetStatDefs != null)
            {
                foreach (StatDef statDef in targetStatDefs)
                {
                    modifierStat *= target.GetStatValue(statDef);
                }
            }

            if (attackerStatDefs != null && instigator != null)
            {
                foreach (StatDef statDef in attackerStatDefs)
                {
                    modifierStat *= instigator.GetStatValue(statDef);
                }
            }

            return (modifier * modifierStat, offset);
        }
    }
}
