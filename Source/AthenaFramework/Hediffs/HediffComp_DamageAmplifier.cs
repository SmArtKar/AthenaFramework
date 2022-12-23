using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
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

        public virtual (float, float) GetDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator = null)
        {
            float modifier = 1f;
            float offset = 0f;
            List<string> excluded = new List<string>();

            foreach (AmplificationType modGroup in Props.modifiers)
            {
                (float, float) result = modGroup.GetDamageModifiers(target, ref excluded, ref excludedGlobal, instigator);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }
    }

    public class HediffCompProperties_DamageAmplifier : HediffCompProperties
    {
        public HediffCompProperties_DamageAmplifier()
        {
            this.compClass = typeof(HediffComp_DamageAmplifier);
        }

        // List of possible amplification effects
        public List<AmplificationType> modifiers = new List<AmplificationType>();
        // Passive damage modifier that's always applied
        public float damageMultiplier = 1f;
    }
}
