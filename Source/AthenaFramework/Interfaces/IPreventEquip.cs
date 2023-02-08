using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AthenaFramework
{
    public interface IPreventEquip
    {
        public abstract bool PreventEquip(Pawn pawn, out string cantReason);

        // Must be added to AthenaCache.equipCache to work
        // AthenaCache.AddCache(this, AthenaCache.equipCache, parent.thingIDNumber)
    }
}
