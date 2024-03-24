using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class GeneButcherProductExtension : DefModExtension
    {
        public Dictionary<ThingDef, int> additionalDrops;
        public bool affectedByEfficiency = false;
    }
}
