using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

[DefOf]
public static class AthenaDefOf
{
    public static JobDef Athena_ReloadAbility;
    public static StatDef Athena_Metabolism;
    public static StatDef Athena_SkillLoss;
    public static HediffDef Athena_DroneHandler;

    static AthenaDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(AthenaDefOf));
    }
}
