using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public interface IStatModifier
    {
        public abstract void GearStatOffset(StatDef stat, ref float result);

        public abstract bool GearAffectsStat(StatDef stat);

        public abstract void GetValueOffsets(StatWorker worker, StatRequest req, bool applyPostProcess, ref float result);

        public abstract void GetValueFactors(StatWorker worker, StatRequest req, bool applyPostProcess, ref float result);

        // Must be added to AthenaCache.statmodCache to work
        // AthenaCache.AddCache(this, AthenaCache.statmodCache, parent.thingIDNumber)
    }
}
