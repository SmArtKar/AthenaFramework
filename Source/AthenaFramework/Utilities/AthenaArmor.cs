using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public static class AthenaArmor
    {
        public static bool CoversBodyPart(Thing armor, ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn)
        {
            bool defaultCover = armor.def.apparel.CoversBodyPart(part);
            bool currentCover = defaultCover;

            bool haveForceCover = false;
            bool haveForceUncover = false;

            if (!AthenaCache.armorCache.TryGetValue(armor.thingIDNumber, out List<IArmored> mods))
            {
                return defaultCover;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                if (mods[i].CoversBodypart(ref amount, armorPenetration, stat, ref armorRating, part, ref damageDef, pawn, defaultCover, out bool forceCover, out bool forceUncover))
                {
                    currentCover = true;
                }

                if (forceCover)
                {
                    haveForceCover = true;
                }

                if (forceUncover)
                {
                    haveForceUncover = true;
                }
            }

            if (haveForceCover)
            {
                return true;
            }

            if (haveForceUncover)
            {
                return false;
            }

            return currentCover;
        }

        public static void ApplyArmor(Thing armor, ref float amount, float armorPenetration, StatDef stat, float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, out bool metalArmor)
        {
            DamageDef originalDamageDef = damageDef;
            float originalAmount = amount;

            bool foundMods = AthenaCache.armorCache.TryGetValue((armor ?? pawn).thingIDNumber, out List<IArmored> mods);
            metalArmor = false;

            if (foundMods)
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    if (!mods[i].PreProcessArmor(ref amount, armorPenetration, stat, ref armorRating, part, ref damageDef, pawn, ref metalArmor, originalDamageDef, originalAmount))
                    {
                        return;
                    }
                }
            }

            ArmorUtility.ApplyArmor(ref amount, armorPenetration, armorRating, armor, ref damageDef, pawn, out bool isMetalArmor);

            if (isMetalArmor)
            {
                metalArmor = true;
            }

            if (foundMods)
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    mods[i].PostProcessArmor(ref amount, armorPenetration, stat, ref armorRating, part, ref damageDef, pawn, ref metalArmor, originalDamageDef, originalAmount);
                }
            }
        }
    }
}
