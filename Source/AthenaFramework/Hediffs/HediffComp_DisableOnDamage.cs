using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class HediffComp_DisableOnDamage : HediffComp
    {
        private HediffCompProperties_DisableOnDamage Props => props as HediffCompProperties_DisableOnDamage;

        public virtual bool ShouldIncreaseDuration
        {
            get
            {
                return Props.increaseDisabledDuration && (Props.disabledDurationTicks > 0 || Props.durationScaling != null);
            }
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            int duration = GetDisabledDuration(dinfo, totalDamageDealt);

            if (duration == -1)
            {
                return;
            }

            if (Props.replacementDef == null)
            {
                Log.Error("Attempted to use HediffComp_DisableOnDamage on " + parent.def.defName + " without specifying a replacementDef");
                return;
            }

            HediffWithComps replacer = HediffMaker.MakeHediff(Props.replacementDef, Pawn, parent.Part) as HediffWithComps;
            HediffComp_HediffRestorer restorer = replacer.TryGetComp<HediffComp_HediffRestorer>();

            if (restorer == null)
            {
                Log.Error("Attempted to use HediffComp_DisableOnDamage on " + parent.def.defName + " with " + Props.replacementDef.defName + " missing HediffComp_HediffRestorer");
                return;
            }

            restorer.hediffToRestore = parent;
            restorer.ticksToRemove = duration;
            Pawn.health.RemoveHediff(parent);
        }

        public virtual int GetDisabledDuration(DamageInfo dinfo, float totalDamageDealt)
        {
            if (totalDamageDealt < Props.minDamage)
            {
                return -1;
            }

            if (Props.damageDefWhitelist != null && !Props.damageDefWhitelist.Contains(dinfo.Def))
            {
                return -1;
            }

            if (Props.damageDefBlacklist != null && Props.damageDefBlacklist.Contains(dinfo.Def))
            {
                return -1;
            }

            if (Props.durationScaling != null)
            {
                return (int)Props.durationScaling.Evaluate(totalDamageDealt);
            }

            return Props.disabledDurationTicks;
        }
    }

    public class HediffCompProperties_DisableOnDamage : HediffCompProperties
    {
        public HediffCompProperties_DisableOnDamage()
        {
            this.compClass = typeof(HediffComp_DisableOnDamage);
        }

        // Def of a hediff that will replace this hediff upon being disabled
        public HediffDef replacementDef;
        // Minimal amount of damage required to disable the hediff
        public float minDamage = 0f;
        // After what time the hediff should be reenabled. When set to -1, parent hediff will only be reenabled when the replacement hediff is disabled
        public int disabledDurationTicks = -1;
        // When set to true, disabled duration will be increased even when the hediff is already disabled
        public bool increaseDisabledDuration = true;
        // When set, disabledDurationTicks will be replaced by values from this curve with X being the amount of damage dealt
        public SimpleCurve durationScaling;
        // White and black lists for damageDefs. Defaults to all damage defs
        public List<DamageDef> damageDefWhitelist;
        public List<DamageDef> damageDefBlacklist;
    }
}
