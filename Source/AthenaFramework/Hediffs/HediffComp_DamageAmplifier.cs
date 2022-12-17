using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_DamageAmplifier : HediffComp
    {
        private HediffCompProperties_DamageAmplifier Props => props as HediffCompProperties_DamageAmplifier;

        public virtual float DamageMultiplier
        {
            get
            {
                return Props.damageMultiplier;
            }
        }
    }

    public class HediffCompProperties_DamageAmplifier : HediffCompProperties
    {
        public HediffCompProperties_DamageAmplifier()
        {
            this.compClass = typeof(HediffComp_DamageAmplifier);
        }

        public float damageMultiplier = 1f;
    }
}
