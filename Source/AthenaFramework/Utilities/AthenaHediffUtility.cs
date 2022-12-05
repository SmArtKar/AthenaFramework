using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public static class AthenaHediffUtility
    {
        public static Dictionary<Pawn, List<CompHediff_DamageAmplifier>> amplifierCompsByPawn = new Dictionary<Pawn, List<CompHediff_DamageAmplifier>>();
        public static Dictionary<Pawn, List<CompHediff_Renderable>> renderableCompsByPawn = new Dictionary<Pawn, List<CompHediff_Renderable>>();
    }
}
