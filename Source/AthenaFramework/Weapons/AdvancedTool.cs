using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class AdvancedTool : Tool
    {
        public virtual void DamageModification(Verb verb, ref DamageInfo dinfo, LocalTargetInfo target, Pawn caster, out IEnumerator<DamageInfo> additionalDamage) { additionalDamage = (IEnumerator<DamageInfo>)Enumerable.Empty<DamageInfo>(); }

        public virtual void TargetModification(Verb verb, ref LocalTargetInfo target) { }

        public virtual void CooldownModification(VerbProperties verbProps, ref float cooldown, Tool tool, Pawn attacker, Thing equipment) { }
    }
}
