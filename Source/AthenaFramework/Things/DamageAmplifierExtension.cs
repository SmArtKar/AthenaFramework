using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class DamageAmplifierExtension : DefModExtension
    {
        // List of possible amplification effects
        public List<AmplificationType> modifiers = new List<AmplificationType>();
        // Passive damage modifier that's always applied
        public float damageMultiplier = 1f;

        public virtual (float, float) GetDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator = null)
        {
            float modifier = 1f;
            float offset = 0f;
            List<string> excluded = new List<string>();

            foreach (AmplificationType modGroup in modifiers)
            {
                (float, float) result = modGroup.GetDamageModifiers(target, ref excluded, ref excludedGlobal, instigator);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }
    }
}
