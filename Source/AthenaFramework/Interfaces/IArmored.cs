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
        // Return False to halt the armor calculations
        public abstract bool PreProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount);

        public abstract void PostProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount);

        public abstract bool CoversBodypart(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, bool defaultCovered, out bool forceCover, out bool forceUncover);

        // Must be added to AthenaCache.armorCache to work
        // AthenaCache.AddCache(this, AthenaCache.armorCache, pawn.thingIDNumber)
    }
}
