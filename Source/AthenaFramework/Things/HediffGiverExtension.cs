using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffGiverExtension : DefModExtension
    {
        public List<HediffBodypartPair> bodypartPairs;
    }

    public class HediffBodypartPair
    {
        public BodyPartDef bodyPartDef;
        public HediffDef hediffDef;
    }
}
