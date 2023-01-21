using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace AthenaFramework
{
    public class HediffComp_Armored : HediffComp, IArmored
    {
        private HediffCompProperties_Armored Props => props as HediffCompProperties_Armored;

        public virtual void ApplyArmor(ref float amount, float armorPenetration, StatDef armorStat, BodyPartRecord part, ref DamageDef damageDef, out bool metalArmor)
        {
            bool blocksDamage = false;

            if (part != parent.Part || parent.Part == null)
            {
                blocksDamage = true;
            }

            if (Props.additionalGroups != null && part.groups.Intersect(Props.additionalGroups).Count() > 0)
            {
                blocksDamage = true;
            }

            if (Props.protectChildren && parent.Part.GetPartAndAllChildParts().Contains(part))
            {
                blocksDamage = true;
            }

            if (!blocksDamage)
            {
                metalArmor = false;
                return;
            }

            float armorRating = 0f;

            for (int i = Props.armorStats.Count - 1; i >= 0; i--)
            {
                StatModifier stat = Props.armorStats[i];

                if (stat.stat == armorStat)
                {
                    armorRating = stat.value;
                    break;
                }
            }

            metalArmor = Props.metallicBlock; 

            float blockLeft = Mathf.Max(armorRating - armorPenetration, 0f);
            float randomValue = Rand.Value;

            if (randomValue < blockLeft * 0.5)
            {
                amount = 0f;
                return;
            }

            if (randomValue < blockLeft)
            {
                amount /= 2;

                if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    damageDef = DamageDefOf.Blunt;
                }
            }

            switch (Props.durabilityMode)
            {
                case HediffDurability.LowerSeverity:
                    parent.Severity -= amount * Props.durabilityCoeff * 0.01f;
                    break;

                case HediffDurability.IncreaseSeverity:
                    parent.Severity += amount * Props.durabilityCoeff * 0.01f;
                    break;
            }
        }
    }

    public class HediffCompProperties_Armored : HediffCompProperties
    {
        public HediffCompProperties_Armored()
        {
            this.compClass = typeof(HediffComp_Armored);
        }

        // List of additional body part groups that are protected aside from the hediff's bodypart
        public List<BodyPartGroupDef> additionalGroups;
        // If the hediff should not only protect the parent part, but also all child parts
        public bool protectChildren = true;
        // If block should be considered metallic for VFX purposes
        public bool metallicBlock = false;
        // What type of durability should the hediff use
        public HediffDurability durabilityMode = HediffDurability.None;
        // Coefficent for the settings above. Uses percentages for severity, 0.25 would result in 400 damage required to destroy the hediff.
        public float durabilityCoeff = 0.25f;
        // List of armor stats
        public List<StatModifier> armorStats = new List<StatModifier>();
    }

    public enum HediffDurability
    {
        None,
        LowerSeverity,     //Lowers hediff severity, uses durabilityCoeff
        IncreaseSeverity   //Increases hediff severity, uses durabilityCoeff
    }
}