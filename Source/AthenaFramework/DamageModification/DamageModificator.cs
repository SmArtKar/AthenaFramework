using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DamageModificator
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
        // List of genes that are would trigger the effect
        public List<GeneDef> geneDefs;
        // List of mutually exclusive effects. If any effect with a common element in this field has been applied, this effect won't apply
        public List<string> excluded;
        // Same as above, but applies to all damage modification from this pawn/object, it's apparel, it's hediffs, etc. Does not intercept with excluded.
        public List<string> excludedGlobal;
        // List of target's StatDefs that multiply the damage
        public List<StatDef> targetStatDefs;
        // List of attacker's StatDefs that multiply the damage
        public List<StatDef> attackerStatDefs;
        // List of whitelisted DamageDefs. When set, DamageDefs that are not in this list won't be affected.
        public List<DamageDef> whitelistedDamageDefs;
        // List of blacklisted DamageDefs. When set, DamageDefs that are in this list won't be affected.
        public List<DamageDef> blacklistedDamageDefs;
        // Whenever the effect changes ranged/explosive/melee damage
        public bool affectRangedDamage = true;
        public bool affectExplosions = true;
        public bool affectMeleeDamage = true;
        // Number by which the damage is modified when the conditions above are met
        public float modifier = 1f;
        // Number by which the damage is passively offset when the conditions above are met. Applied after all modifiers.
        public float offset = 0f;

        public virtual (float, float) GetDamageModifiers(Thing target, ref List<string> excludedLocal, ref List<string> excludedGlobalInput, Thing instigator, DamageInfo? dinfo = null, bool projectile = false, bool incoming = false)
        {
            if (excluded != null)
            {
                if (excludedLocal.Intersect(excluded).Count() > 0)
                {
                    return (1f, 0f);
                }
            }

            if (excludedGlobal != null)
            {
                if (excludedGlobalInput.Intersect(excludedGlobal).Count() > 0)
                {
                    return (1f, 0f);
                }
            }

            if (dinfo != null)
            {
                DamageInfo trueDinfo = (DamageInfo)dinfo;
                if ((trueDinfo.Def.isRanged && !affectRangedDamage) || (trueDinfo.Def.isExplosive && !affectExplosions) || (!trueDinfo.Def.isRanged && !trueDinfo.Def.isExplosive && !affectMeleeDamage))
                {
                    return (1f, 0f);
                }

                if (whitelistedDamageDefs != null && !whitelistedDamageDefs.Contains(trueDinfo.Def))
                {
                    return (1f, 0f);
                }

                if (blacklistedDamageDefs != null && blacklistedDamageDefs.Contains(trueDinfo.Def))
                {
                    return (1f, 0f);
                }

            }

            if (projectile && !affectRangedDamage)
            {
                return (1f, 0f);
            }

            if (thingDefs != null && !thingDefs.Contains(target.def))
            {
                return (1f, 0f);
            }

            if (factionDef != null && (target.Faction == null || target.Faction.def != factionDef))
            {
                return (1f, 0f);
            }

            if (fleshTypes != null || pawnKinds != null || hediffDefs != null || geneDefs != null)
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
                    List<HediffDef> localHediffDefs = new List<HediffDef>(hediffDefs);
                    for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                    {
                        Hediff hediff = pawn.health.hediffSet.hediffs[i];

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

                if (geneDefs != null)
                {
                    if (pawn.genes == null)
                    {
                        return (1f, 0f);
                    }

                    bool foundGene = false;

                    for (int i = geneDefs.Count - 1; i >= 0; i--)
                    {
                        if (pawn.genes.HasGene(geneDefs[i]))
                        {
                            foundGene = true;
                            break;
                        }
                    }

                    if (!foundGene)
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

            if (incoming)
            {
                if (targetStatDefs != null)
                {
                    for (int i = targetStatDefs.Count - 1; i >= 0; i--)
                    {
                        StatDef statDef = targetStatDefs[i];
                        modifierStat *= instigator.GetStatValue(statDef);
                    }
                }

                if (target != null)
                {
                    if (attackerStatDefs != null && instigator != null)
                    {
                        for (int i = attackerStatDefs.Count - 1; i >= 0; i--)
                        {
                            StatDef statDef = attackerStatDefs[i];
                            modifierStat *= target.GetStatValue(statDef);
                        }
                    }
                }
            }
            else
            {
                if (targetStatDefs != null)
                {
                    for (int i = targetStatDefs.Count - 1; i >= 0; i--)
                    {
                        StatDef statDef = targetStatDefs[i];
                        modifierStat *= target.GetStatValue(statDef);
                    }
                }

                if (instigator != null)
                {
                    if (attackerStatDefs != null && instigator != null)
                    {
                        for (int i = targetStatDefs.Count - 1; i >= 0; i--)
                        {
                            StatDef statDef = attackerStatDefs[i];
                            modifierStat *= instigator.GetStatValue(statDef);
                        }
                    }
                }
            }

            return (modifier * modifierStat, offset);
        }
    }
}
