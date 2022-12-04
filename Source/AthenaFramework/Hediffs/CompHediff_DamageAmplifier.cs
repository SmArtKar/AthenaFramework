﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class CompHediff_DamageAmplifier : HediffComp
    {
        public HediffCompProperties_DamageAmplifier Props => props as HediffCompProperties_DamageAmplifier;
    }

    public class HediffCompProperties_DamageAmplifier : HediffCompProperties
    {
        public HediffCompProperties_DamageAmplifier()
        {
            this.compClass = typeof(CompHediff_DamageAmplifier);
        }

        public float damageMultiplier = 1f;
    }
}
