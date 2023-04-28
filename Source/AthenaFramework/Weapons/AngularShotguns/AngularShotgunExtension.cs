using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class AngularShotgunExtension : DefModExtension
    {
        // Amount of pellets that your shotgun fires
        public int pelletCount;
        // Angle between fired pellets
        public float pelletAngle;
        // DEPRECATED
        public float? downedHitChance;

        public override IEnumerable<string> ConfigErrors()
        {
            if (downedHitChance != null)
            {
                Log.Warning("Some mod is using downedHitChance which is deprecated and no longer in use. Please contact the mod's author and ask him to remove the said field.");
            }

            return base.ConfigErrors();
        }
    }
}
