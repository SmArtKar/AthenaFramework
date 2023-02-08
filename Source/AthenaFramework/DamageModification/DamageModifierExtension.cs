using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class DamageModifierExtension : DefModExtension, IDamageModifier
    {
        public virtual float OutgoingDamageMultiplier
        {
            get
            {
                return outgoingDamageMultiplier;
            }
        }

        public virtual (float, float) GetOutcomingDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator, DamageInfo? dinfo, bool projectile = false)
        {
            float modifier = 1f;
            float offset = 0f;
            List<string> excluded = new List<string>();

            for (int i = outgoingModifiers.Count - 1; i >= 0; i--)
            {
                DamageModificator modGroup = outgoingModifiers[i];
                (float, float) result = modGroup.GetDamageModifiers(target, ref excluded, ref excludedGlobal, instigator, dinfo, projectile);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }

        public virtual (float, float) GetIncomingDamageModifier(Thing target, ref List<string> excludedGlobal, Thing instigator, DamageInfo? dinfo, bool projectile = false)
        {
            float modifier = incomingDamageMultiplier;
            float offset = 0f;
            List<string> excluded = new List<string>();

            for (int i = incomingModifiers.Count - 1; i >= 0; i--)
            {
                DamageModificator modGroup = incomingModifiers[i];
                (float, float) result = modGroup.GetDamageModifiers(instigator, ref excluded, ref excludedGlobal, target, dinfo, projectile, true);
                modifier *= result.Item1;
                offset += result.Item2;
            }

            return (modifier, offset);
        }

        // List of possible modification effects that affect outgoing damage
        public List<DamageModificator> outgoingModifiers = new List<DamageModificator>();
        // List of possible modification effects that affect outgoing damage
        public List<DamageModificator> incomingModifiers = new List<DamageModificator>();
        // Passive outgoing damage modifier that's always applied
        public float outgoingDamageMultiplier = 1f;
        // Passive incoming damage modifier that's always applied
        public float incomingDamageMultiplier = 1f;
    }
}
