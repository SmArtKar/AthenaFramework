using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Verb_RandomShot : Verb_Shoot
    {
        public RandomShotExtension cachedExtension;

        public RandomShotExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    if (HediffSource != null)
                    {
                        cachedExtension = HediffSource.def.GetModExtension<RandomShotExtension>();
                    } 
                    else
                    {
                        cachedExtension = EquipmentSource.def.GetModExtension<RandomShotExtension>();
                    }
                }

                return cachedExtension;
            }
        }

        public override ThingDef Projectile
        {
            get
            {
                int projectileChanceSum = 0;

                for (int i = Extension.projectiles.Count - 1; i >= 0; i--)
                {
                    projectileChanceSum += Extension.projectiles[i].probability;
                }

                int value = Rand.RangeInclusive(0, projectileChanceSum);
                int counter = 0;

                for (int i = Extension.projectiles.Count - 1; i >= 0; i--)
                {
                    bool first = value > counter;
                    counter += Extension.projectiles[i].probability;
                    if (first && value <= counter)
                    {
                        return Extension.projectiles[i].projectile;
                    }
                }

                return Extension.projectiles.Last().projectile;
            }
        }
    }

    public class RandomShotExtension : DefModExtension
    {
        public List<RandomProjectilePackage> projectiles = new List<RandomProjectilePackage>();
    }

    public class RandomProjectilePackage
    {
        public ThingDef projectile;
        public int probability = 1;
    }
}
