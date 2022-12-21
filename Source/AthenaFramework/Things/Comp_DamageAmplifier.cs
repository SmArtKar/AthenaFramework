using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
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

        public virtual float GetDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator = null)
        {
            float modifier = 1f;
            List<string> excluded = new List<string>();

            foreach(AmplificationType modGroup in Props.modifiers)
            {
                if (modGroup.excluded != null)
                {
                    if (excluded.Intersect(modGroup.excluded).ToList().Count > 0)
                    {
                        continue;
                    }
                }

                if (modGroup.excludedGlobal != null)
                {
                    if (excludedGlobal.Intersect(modGroup.excludedGlobal).ToList().Count > 0)
                    {
                        continue;
                    }
                }

                if (modGroup.thingDefs != null && !modGroup.thingDefs.Contains(target.def))
                {
                    continue;
                }

                if (modGroup.factionDef != null && (target.Faction == null || target.Faction.def != modGroup.factionDef))
                {
                    continue;
                }

                if (modGroup.fleshTypes != null || modGroup.pawnKinds != null || modGroup.hediffDefs != null)
                {
                    if (target is not Pawn)
                    {
                        continue;
                    }

                    Pawn pawn = target as Pawn;

                    if (modGroup.fleshTypes != null && !modGroup.fleshTypes.Contains(pawn.def.race.FleshType))
                    {
                        continue;
                    }

                    if (modGroup.pawnKinds != null && !modGroup.pawnKinds.Contains(pawn.kindDef))
                    {
                        continue;
                    }

                    if (modGroup.hediffDefs != null)
                    {
                        List<HediffDef> hediffDefs = new List<HediffDef>(modGroup.hediffDefs);
                        foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                        {
                            if (hediffDefs.Contains(hediff.def))
                            {
                                hediffDefs.Remove(hediff.def);
                            }
                        }

                        if (hediffDefs.Count > 0)
                        {
                            continue;
                        }
                    }
                }

                if (modGroup.excluded != null)
                {
                    excluded = excluded.Concat(modGroup.excluded).ToList();
                }

                if (modGroup.excludedGlobal != null)
                {
                    excludedGlobal = excludedGlobal.Concat(modGroup.excludedGlobal).ToList();
                }

                if (modGroup.targetStatDefs != null)
                {
                    foreach (StatDef statDef in modGroup.targetStatDefs)
                    {
                        modifier *= target.GetStatValue(statDef);
                    }
                }

                if (modGroup.attackerStatDefs != null && instigator != null)
                {
                    foreach (StatDef statDef in modGroup.attackerStatDefs)
                    {
                        modifier *= instigator.GetStatValue(statDef);
                    }
                }

                modifier *= modGroup.modifier;
            }

            return modifier;
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
        // Same as above, but applies to all damage amplification from this pawn/object, it's apparel, it's hediffs, etc. Does not intersect with excluded.
        public List<string> excludedGlobal;
        // List of target's StatDefs that affect the damage
        public List<StatDef> targetStatDefs;
        // List of attacker's StatDefs that affect the damage
        public List<StatDef> attackerStatDefs;
        // Number by which the damage is modified when the conditions above are met
        public float modifier = 1f;
    }
}
