using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using Mono.Posix;

namespace AthenaFramework
{
    public class HediffComp_Armored : HediffComp, IArmored
    {
        private HediffCompProperties_Armored Props => props as HediffCompProperties_Armored;

        public override string CompTipStringExtra
        {
            get
            {
                string resStr = "";

                for (int i = Props.armorStats.Count - 1; i >= 0; i--)
                {
                    StatModifier statMod = Props.armorStats[i];
                    resStr = resStr + "{0}: {1}{2}% \n".Formatted(statMod.stat.LabelCap, statMod.value > 0 ? "+" : "", statMod.value * 100f);
                }

                for (int i = Props.defArmors.Count - 1; i >= 0; i--)
                {
                    DamageDefArmor defArmor = Props.defArmors[i];
                    resStr = resStr + "{0}: {1}{2}% \n".Formatted(defArmor.damageDef.LabelCap, defArmor.value > 0 ? "+" : "", defArmor.value * 100f);
                }

                return resStr;
            }
        }

        public override void CompPostMake()
        {
            base.CompPostMake();
            AthenaCache.AddCache(this, AthenaCache.armorCache, Pawn.thingIDNumber);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();

            if (Scribe.mode == LoadSaveMode.ResolvingCrossRefs)
            {
                AthenaCache.AddCache(this, AthenaCache.armorCache, Pawn.thingIDNumber);
            }
        }

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();
            AthenaCache.RemoveCache(this, AthenaCache.armorCache, Pawn.thingIDNumber);
        }

        public virtual bool PreProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount) { return true; }

        public virtual void PostProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount)
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

            float localArmorRating = 0f;

            for (int i = Props.armorStats.Count - 1; i >= 0; i--)
            {
                StatModifier localStat = Props.armorStats[i];

                if (localStat.stat == stat)
                {
                    localArmorRating = localStat.value;
                    break;
                }
            }

            for (int j = Props.defArmors.Count - 1; j >= 0; j--)
            {
                DamageDefArmor localStat = Props.defArmors[j];

                if (localStat.damageDef == damageDef)
                {
                    armorRating += localStat.value;
                    break;
                }
            }

            metalArmor = Props.metallicBlock;

            float blockLeft = Mathf.Max(localArmorRating - armorPenetration, 0f);
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

        public virtual bool CoversBodypart(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, bool defaultCovered, out bool forceCover, out bool forceUncover)
        {
            forceCover = false;
            forceUncover = false;

            return false;
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
        // List of armor per damageDefs
        public List<DamageDefArmor> defArmors = new List<DamageDefArmor>();
    }

    public enum HediffDurability
    {
        None,
        LowerSeverity,     //Lowers hediff severity, uses durabilityCoeff
        IncreaseSeverity   //Increases hediff severity, uses durabilityCoeff
    }
}