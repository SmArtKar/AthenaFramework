using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public interface IProjectile
    {

        public abstract void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef);

        public abstract void Impact(Thing hitThing, ref bool blockedByShield);

        public abstract void CanHit(Thing hitThing, ref bool result);

        // Must be added to AthenaCache.projectileCache to work
        // AthenaCache.AddCache(this, AthenaCache.projectileCache, parent.thingIDNumber)
    }
}
