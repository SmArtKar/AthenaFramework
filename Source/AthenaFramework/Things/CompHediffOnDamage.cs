using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompHediffOnDamage : ThingComp
    {
        private CompProperties_HediffOnDamage Props => props as CompProperties_HediffOnDamage;
        private Pawn Pawn => parent as Pawn;

        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);

            if ((dinfo.Def.isRanged && !Props.triggeredByRangedDamage) || (dinfo.Def.isExplosive && !Props.triggeredByExplosions) || (!dinfo.Def.isRanged && !dinfo.Def.isExplosive && !Props.triggeredByMeleeDamage))
            {
                return;
            }

            if (Props.whitelistedDamageDefs != null && !Props.whitelistedDamageDefs.Contains(dinfo.Def))
            {
                return;
            }

            if (Props.blacklistedDamageDefs != null && Props.blacklistedDamageDefs.Contains(dinfo.Def))
            {
                return;
            }

            Hediff hediff = null;

            for (int i = Pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff tempHediff = Pawn.health.hediffSet.hediffs[i];

                if (Props.applyToBodypart && tempHediff.Part != dinfo.HitPart)
                {
                    continue;
                }

                if (tempHediff.def == Props.givenHediff)
                {
                    hediff = tempHediff;
                    break;
                }
            }

            if (hediff != null)
            {
                if (Props.severityPerDamage != null)
                {
                    hediff.Severity += (float)Props.severityPerDamage * dinfo.Amount;
                }

                return;
            }

            hediff = Pawn.health.AddHediff(Props.givenHediff);

            if (Props.severityPerDamage != null)
            {
                hediff.Severity = (float)Props.severityPerDamage * dinfo.Amount;
            }
        }
    }

    public class CompProperties_HediffOnDamage : CompProperties
    {
        public CompProperties_HediffOnDamage()
        {
            this.compClass = typeof(CompHediffOnDamage);
        }

        public HediffDef givenHediff;

        // When set, hediff's severity will be adjusted by damage amount multiplied by this number
        public float? severityPerDamage;
        // If hediff should be applied to the damaged bodypart or to the whole body
        public bool applyToBodypart = false;

        // Whenever the comp triggers on ranged/explosive/melee damage
        public bool triggeredByRangedDamage = true;
        public bool triggeredByExplosions = true;
        public bool triggeredByMeleeDamage = true;

        // List of whitelisted DamageDefs. When set, DamageDefs that are not in this list won't be affected.
        public List<DamageDef> whitelistedDamageDefs;

        // List of blacklisted DamageDefs. When set, DamageDefs that are in this list won't be affected.
        public List<DamageDef> blacklistedDamageDefs;
    }
}
