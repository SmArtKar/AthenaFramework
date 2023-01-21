using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class ProjectileComp : ThingComp
    {
        protected Projectile Projectile => parent as Projectile;

        public virtual void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef) { }

        public virtual void Impact(Thing hitThing, ref bool blockedByShield) { }

        public virtual void CanHit(Thing hitThing, ref bool result) { }
    }
}