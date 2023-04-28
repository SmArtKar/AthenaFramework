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
        public virtual void DamageModification(Verb verb, ref float damage, ref float armorPenetration, ref LocalTargetInfo target, Pawn caster) { }

        public virtual void CooldownModification(VerbProperties verbProps, ref float cooldown, Tool tool, Pawn attacker, Thing equipment) { }
    }
}
