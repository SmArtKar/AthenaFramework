using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class HediffComp_DisableOnDamage : HediffComp, IDamageResponse
    {
        private HediffCompProperties_DisableOnDamage Props => props as HediffCompProperties_DisableOnDamage;

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            if (Props.replacementDef == null)
            {
                Log.Error("Attempted to use HediffComp_DisableOnDamage on " + parent.def.defName + " without specifying a replacementDef");
                return;
            }

            if (totalDamageDealt < Props.minDamage)
            {
                return;
            }

            if (Props.damageDefWhitelist != null && !Props.damageDefWhitelist.Contains(dinfo.Def))
            {
                return;
            }

            if (Props.damageDefBlacklist != null && Props.damageDefBlacklist.Contains(dinfo.Def))
            {
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
            Pawn.health.RemoveHediff(parent);
        }

        public virtual void PreApplyDamage(ref DamageInfo dinfo, ref bool absorbed) { }
    }

    public class HediffCompProperties_DisableOnDamage : HediffCompProperties
    {
        public HediffCompProperties_DisableOnDamage()
        {
            this.compClass = typeof(HediffComp_DisableOnDamage);
        }

        //Def of a hediff that will replace this hediff upon being disabled
        public HediffDef replacementDef;
        // Minimal amount of damage required to disable the hediff
        public float minDamage = 0f;
        // White and black lists for damageDefs. Defaults to all damage defs
        public List<DamageDef> damageDefWhitelist;
        public List<DamageDef> damageDefBlacklist;
    }
}
