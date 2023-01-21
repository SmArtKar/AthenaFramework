using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public interface IArmored
    {
        public abstract void ApplyArmor(ref float amount, float armorPenetration, StatDef armorStat, BodyPartRecord part, ref DamageDef damageDef, out bool metalArmor);
    }
}
