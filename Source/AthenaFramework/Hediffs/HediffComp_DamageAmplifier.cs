using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_DamageAmplifier : HediffComp
    {
        private HediffCompProperties_DamageAmplifier Props => props as HediffCompProperties_DamageAmplifier;

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

            foreach (AmplificationType modGroup in Props.modifiers)
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
                    if (target is not Verse.Pawn)
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

    public class HediffCompProperties_DamageAmplifier : HediffCompProperties
    {
        public HediffCompProperties_DamageAmplifier()
        {
            this.compClass = typeof(HediffComp_DamageAmplifier);
        }

        // List of possible amplification effects
        public List<AmplificationType> modifiers = new List<AmplificationType>();
        // Passive damage modifier that's always applied
        public float damageMultiplier = 1f;
    }
}
