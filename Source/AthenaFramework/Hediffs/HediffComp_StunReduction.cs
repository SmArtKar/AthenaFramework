using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_StunReduction : HediffComp
    {
        private HediffCompProperties_StunReduction Props => props as HediffCompProperties_StunReduction;

        public virtual (float, float) GetStunModifiers(DamageInfo dinfo)
        {
            float offset = Props.offset;
            float modifier = Props.modifier;

            for (int i = Props.stunMods.Count - 1; i >= 0; i--)
            {
                StunMod mod = Props.stunMods[i];
                if (dinfo.Def == mod.damageDef)
                {
                    offset += mod.offset;
                    modifier *= mod.modifier;
                }
            }

            return (offset, modifier);
        }

        public virtual bool BlockStun(DamageInfo dinfo)
        {
            if (Props.blockStuns.Contains(dinfo.Def))
            {
                return true;
            }

            return false;
        }
    }

    public class HediffCompProperties_StunReduction : HediffCompProperties
    {
        public HediffCompProperties_StunReduction()
        {
            this.compClass = typeof(HediffComp_StunReduction);
        }

        // List of damage defs that are blocked entirely
        public List<DamageDef> blockStuns;
        // Passive offset that's applied to all stuns. Applied after modifiers
        public float offset = 0f;
        // Passive multiplier that's applied to all stuns
        public float modifier = 1f;
        // List of offsets and modifiers per damage def
        public List<StunMod> stunMods;
    }

    public struct StunMod
{
        public DamageDef damageDef;
        public float offset = 0f;
        public float modifier = 1f;

        public StunMod() { }
    }
}
