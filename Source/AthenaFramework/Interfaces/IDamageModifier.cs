using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public interface IDamageModifier
    {
        public abstract (float, float) GetOutcomingDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator, DamageInfo? dinfo, bool projectile = false);

        public abstract (float, float) GetIncomingDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator, DamageInfo? dinfo, bool projectile = false);

        public abstract float OutgoingDamageMultiplier { get; }

        // Must be added to AthenaCache.damageCache to work
        // AthenaCache.AddCache(this, AthenaCache.damageCache, parent.thingIDNumber)
    }
}
