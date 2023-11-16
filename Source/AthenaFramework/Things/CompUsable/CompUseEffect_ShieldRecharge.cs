using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using System.Reflection;

namespace AthenaFramework
{
    public class CompUseEffect_ShieldRecharge : CompUseEffect
    {
        private new CompProperties_UseEffectShieldRecharge Props => props as CompProperties_UseEffectShieldRecharge;

        public static Type VEFshieldType;
        public static FieldInfo VEFshieldEnergy;
        public static FieldInfo VEFshieldUseEnergy;
        public static FieldInfo VEFshieldResetTicks;
        public static Type VEFshieldPropsType;
        public static FieldInfo VEFshieldMaxEnergy;
        public static MethodInfo getCompMethod;

        public override void DoEffect(Pawn usedBy)
        {
            base.DoEffect(usedBy);

            if (AthenaCache.modDetectedVEF && VEFshieldType == null)
            {
                InitVEFCompat();
            }

            List<CompShield> comps1 = new List<CompShield>();

            CompShield comp1 = usedBy.TryGetComp<CompShield>();

            if (comp1 != null)
            {
                comps1.Add(comp1);
            }

            List<Apparel> apparel = usedBy.apparel.WornApparel;

            for (int i = apparel.Count - 1; i >= 0; i--)
            {
                comp1 = apparel[i].TryGetComp<CompShield>();

                if (comp1 != null)
                {
                    comps1.Add(comp1);
                }
            }

            float energy = Props.rechargeAmount;
            float percentage = Props.rechargePercentage;

            for (int i = comps1.Count - 1; i >= 0; i--)
            {
                comp1 = comps1[i];

                if (Props.overchargeShields)
                {
                    comp1.energy += energy;
                    comp1.energy += comp1.EnergyMax * percentage;
                    return;
                }

                float prevEnergy = comp1.energy;
                comp1.energy = Math.Min(comp1.energy + energy, comp1.EnergyMax);
                energy -= comp1.energy - prevEnergy;
                prevEnergy = comp1.energy;
                comp1.energy = Math.Min(comp1.energy + percentage * comp1.EnergyMax, comp1.EnergyMax);
                percentage -= (comp1.energy - prevEnergy) / comp1.EnergyMax;

                if (!Props.useLeftovers)
                {
                    return;
                }
            }

            for (int i = usedBy.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                HediffWithComps hediff = usedBy.health.hediffSet.hediffs[i] as HediffWithComps;

                if (hediff == null)
                {
                    continue;
                }

                HediffComp_Shield comp2 = hediff.TryGetComp<HediffComp_Shield>();

                if (comp2 != null)
                {
                    if (Props.overchargeShields)
                    {
                        comp2.energy += energy;
                        comp2.energy += comp2.MaxEnergy * percentage;
                        return;
                    }

                    float prevEnergy = comp2.energy;
                    comp2.energy = Math.Min(comp2.energy + energy, comp2.MaxEnergy);
                    energy -= comp2.energy - prevEnergy;
                    prevEnergy = comp2.energy;
                    comp2.energy = Math.Min(comp2.energy + percentage * comp2.MaxEnergy, comp2.MaxEnergy);
                    percentage -= (comp2.energy - prevEnergy) / comp2.MaxEnergy;

                    if (!Props.useLeftovers)
                    {
                        return;
                    }
                }

                if (VEFshieldType == null)
                {
                    continue;
                }

                HediffComp comp3 = Convert.ChangeType(getCompMethod.Invoke(hediff, null), typeof(HediffComp)) as HediffComp;

                if (comp3 == null)
                {
                    continue;
                }

                float maxEnergy = (float)VEFshieldMaxEnergy.GetValue(comp3.props);

                if (Props.overchargeShields)
                {
                    VEFshieldEnergy.SetValue(comp3, (float)VEFshieldEnergy.GetValue(comp3) + energy + maxEnergy * percentage);
                    return;
                }

                float prevEnergy2 = (float)VEFshieldEnergy.GetValue(comp3);
                VEFshieldEnergy.SetValue(comp3, Math.Min(prevEnergy2 + energy, maxEnergy));
                energy -= (float)VEFshieldMaxEnergy.GetValue(comp3) - prevEnergy2;
                prevEnergy2 = (float)VEFshieldEnergy.GetValue(comp3);
                VEFshieldEnergy.SetValue(comp3, Math.Min(prevEnergy2 + percentage * maxEnergy, maxEnergy));
                percentage -= ((float)VEFshieldMaxEnergy.GetValue(comp3) - prevEnergy2) / maxEnergy;

                if (!Props.useLeftovers)
                {
                    return;
                }
            }
        }

        public static void InitVEFCompat()
        {
            VEFshieldType = AccessTools.TypeByName("VFECore.Shields.HediffComp_Shield");
            VEFshieldEnergy = AccessTools.Field(VEFshieldType, "energy");
            VEFshieldUseEnergy = AccessTools.Field(VEFshieldType, "useEnergy");
            VEFshieldResetTicks = AccessTools.Field(VEFshieldType, "ticksTillReset");
            VEFshieldPropsType = AccessTools.TypeByName("VFECore.Shields.HediffCompProperties_Shield");
            VEFshieldMaxEnergy = AccessTools.Field(VEFshieldPropsType, "maxEnergy");
            getCompMethod = AccessTools.Method(typeof(HediffWithComps), "TryGetComp").MakeGenericMethod(VEFshieldType);
        }
    }

    public class CompProperties_UseEffectShieldRecharge : CompProperties_Usable
    {
        public float rechargeAmount = 0f;
        public float rechargePercentage = 0f;

        public bool overchargeShields = false;
        // If pawn somehow has multiple shields active, setting this field to true will make "leftover" charge if the first shield is fully charged to be used on other shields
        // Else, the comp only checks for the first shield it can find and will spend all the charge on it.
        // Does nothing if the field above it set to true;
        public bool useLeftovers = true;

        public CompProperties_UseEffectShieldRecharge()
        {
            this.compClass = typeof(CompUseEffect_ShieldRecharge);
        }
    }
}
