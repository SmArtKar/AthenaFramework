using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Verb_EliteShot : Verb_Shoot
    {
        public int shotsLeft = 0;
        public EliteShotExtension cachedExtension;

        public EliteShotExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    cachedExtension = EquipmentSource.def.GetModExtension<EliteShotExtension>();
                }

                return cachedExtension;
            }
        }

        public Verb_EliteShot()
        {
            shotsLeft = Extension.shotsToElite + 1;
        }

        public override ThingDef Projectile
        {
            get
            {
                if (shotsLeft > 0)
                {
                    return base.Projectile;
                }

                return Extension.eliteShotProjectile;
            }
        }

        public override bool TryCastShot()
        {
            if (shotsLeft == 0)
            {
                shotsLeft = Extension.shotsToElite + 1;
            }

            shotsLeft -= 1;
            return base.TryCastShot();
        }
    }

    public class EliteShotExtension : DefModExtension
    {
        // How many shots must be taken before the elite shot procs
        public int shotsToElite = 5;
        // Projectile that replaces the default one when casting the elite shot
        public ThingDef eliteShotProjectile;
    }
}
