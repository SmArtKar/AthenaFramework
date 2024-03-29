﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public interface IDamageResponse
    {
        public abstract void PreApplyDamage(ref DamageInfo dinfo, ref bool absorbed);

        // Must be added to AthenaCache.responderCache to work
        // AthenaCache.AddCache(this, AthenaCache.responderCache, pawn.thingIDNumber)
    }
}
