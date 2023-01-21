using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public static class HediffStageUtility
    {
        public static void CopyValues(this HediffStage stage, HediffStage newStage)
        {
            stage.minSeverity = newStage.minSeverity;
            stage.label = newStage.label;
            stage.overrideLabel = newStage.overrideLabel;
            stage.untranslatedLabel = newStage.untranslatedLabel;
            stage.becomeVisible = newStage.becomeVisible;
            stage.lifeThreatening = newStage.lifeThreatening;
            stage.tale = newStage.tale;
            stage.vomitMtbDays = newStage.vomitMtbDays;
            stage.deathMtbDays = newStage.deathMtbDays;
            stage.mtbDeathDestroysBrain = newStage.mtbDeathDestroysBrain;
            stage.painFactor = newStage.painFactor;
            stage.painOffset = newStage.painOffset;
            stage.totalBleedFactor = newStage.totalBleedFactor;
            stage.naturalHealingFactor = newStage.naturalHealingFactor;
            stage.forgetMemoryThoughtMtbDays = newStage.forgetMemoryThoughtMtbDays;
            stage.pctConditionalThoughtsNullified = newStage.pctConditionalThoughtsNullified;
            stage.opinionOfOthersFactor = newStage.opinionOfOthersFactor;
            stage.fertilityFactor = newStage.fertilityFactor;
            stage.hungerRateFactor = newStage.hungerRateFactor;
            stage.hungerRateFactorOffset = newStage.hungerRateFactorOffset;
            stage.restFallFactor = newStage.restFallFactor;
            stage.restFallFactorOffset = newStage.restFallFactorOffset;
            stage.socialFightChanceFactor = newStage.socialFightChanceFactor;
            stage.foodPoisoningChanceFactor = newStage.foodPoisoningChanceFactor;
            stage.mentalBreakMtbDays = newStage.mentalBreakMtbDays;
            stage.mentalBreakExplanation = newStage.mentalBreakExplanation;
            stage.allowedMentalBreakIntensities = newStage.allowedMentalBreakIntensities;
            stage.makeImmuneTo = newStage.makeImmuneTo;
            stage.capMods = newStage.capMods;
            stage.hediffGivers = newStage.hediffGivers;
            stage.mentalStateGivers = newStage.mentalStateGivers;
            stage.statOffsets = newStage.statOffsets;
            stage.statFactors = newStage.statFactors;
            stage.multiplyStatChangesBySeverity = newStage.multiplyStatChangesBySeverity;
            stage.statOffsetEffectMultiplier = newStage.statOffsetEffectMultiplier;
            stage.statFactorEffectMultiplier = newStage.statFactorEffectMultiplier;
            stage.capacityFactorEffectMultiplier = newStage.capacityFactorEffectMultiplier;
            stage.disabledWorkTags = newStage.disabledWorkTags;
            stage.overrideTooltip = newStage.overrideTooltip;
            stage.extraTooltip = newStage.extraTooltip;
            stage.partEfficiencyOffset = newStage.partEfficiencyOffset;
            stage.partIgnoreMissingHP = newStage.partIgnoreMissingHP;
            stage.destroyPart = newStage.destroyPart;
        }
    }

    public class StageOverlay
    {
        // All set MTB values, strings, work tags and defs are overwritten and all lists are merged
        public string label;
        public string overrideLabel;
        public string untranslatedLabel;
        public bool? becomeVisible;
        public bool? lifeThreatening;
        public TaleDef tale;
        public float? vomitMtbDays;
        public float? deathMtbDays;
        public bool? mtbDeathDestroysBrain;
        public float painFactor = 1f;
        public float painOffset = 0f;
        public float totalBleedFactor = 1f;
        public float? naturalHealingFactor;
        public float? forgetMemoryThoughtMtbDays;
        public float pctConditionalThoughtsNullified = 0f;
        public float opinionOfOthersFactor = 1f;
        public float fertilityFactor = 1f;
        public float hungerRateFactor = 1f;
        public float hungerRateFactorOffset = 0f;
        public float restFallFactor = 1f;
        public float restFallFactorOffset = 0f;
        public float socialFightChanceFactor = 1f;
        public float foodPoisoningChanceFactor = 1f;
        public float? mentalBreakMtbDays;
        public string mentalBreakExplanation;
        public List<MentalBreakIntensity> allowedMentalBreakIntensities;
        public List<HediffDef> makeImmuneTo;
        public List<PawnCapacityModifier> capMods;
        public List<HediffGiver> hediffGivers;
        public List<MentalStateGiver> mentalStateGivers;
        public List<StatModifier> statOffsets;
        public List<StatModifier> statFactors;
        public bool? multiplyStatChangesBySeverity;
        public StatDef statOffsetEffectMultiplier;
        public StatDef statFactorEffectMultiplier;
        public StatDef capacityFactorEffectMultiplier;
        public WorkTags? disabledWorkTags;
        public string overrideTooltip;
        public string extraTooltip;
        public float partEfficiencyOffset = 0f;
        public bool? partIgnoreMissingHP;

        public virtual HediffStage ModifyHediffStage(HediffStage stage)
        {
            if (label != null)
            {
                stage.label = label;
            }

            if (becomeVisible != null)
            {
                stage.becomeVisible = (bool)becomeVisible;
            }

            if (untranslatedLabel != null)
            {
                stage.untranslatedLabel = untranslatedLabel;
            }

            if (lifeThreatening != null)
            {
                stage.lifeThreatening = (bool)lifeThreatening;
            }

            if (tale != null)
            {
                stage.tale = tale;
            }

            if (vomitMtbDays != null)
            {
                stage.vomitMtbDays = (float)vomitMtbDays;
            }

            if (deathMtbDays != null)
            {
                stage.deathMtbDays = (float)deathMtbDays;
            }

            if (mtbDeathDestroysBrain != null)
            {
                stage.mtbDeathDestroysBrain = (bool)mtbDeathDestroysBrain;
            }

            stage.painFactor *= painFactor;
            stage.painOffset += painOffset;
            stage.totalBleedFactor *= totalBleedFactor;

            if (naturalHealingFactor != null)
            {
                if (stage.naturalHealingFactor == -1)
                {
                    stage.naturalHealingFactor = (float)naturalHealingFactor;
                }
                else
                {
                    stage.naturalHealingFactor *= (float)naturalHealingFactor;
                }
            }

            if (forgetMemoryThoughtMtbDays != null)
            {
                stage.forgetMemoryThoughtMtbDays = (float)forgetMemoryThoughtMtbDays;
            }

            stage.pctConditionalThoughtsNullified += pctConditionalThoughtsNullified;
            stage.opinionOfOthersFactor *= opinionOfOthersFactor;
            stage.fertilityFactor *= fertilityFactor;
            stage.hungerRateFactor *= hungerRateFactor;
            stage.hungerRateFactorOffset += hungerRateFactorOffset;
            stage.restFallFactor *= restFallFactor;
            stage.restFallFactorOffset += restFallFactorOffset;
            stage.socialFightChanceFactor *= socialFightChanceFactor;
            stage.foodPoisoningChanceFactor *= foodPoisoningChanceFactor;

            if (mentalBreakMtbDays != null)
            {
                stage.mentalBreakMtbDays = (float)mentalBreakMtbDays;
            }

            if (mentalBreakExplanation != null)
            {
                stage.mentalBreakExplanation = mentalBreakExplanation;
            }

            if (allowedMentalBreakIntensities != null)
            {
                if (stage.allowedMentalBreakIntensities != null)
                {
                    stage.allowedMentalBreakIntensities = stage.allowedMentalBreakIntensities.Concat(allowedMentalBreakIntensities).ToList();
                }
                else
                {
                    stage.allowedMentalBreakIntensities = allowedMentalBreakIntensities;
                }
            }

            if (makeImmuneTo != null)
            {
                if (stage.makeImmuneTo != null)
                {
                    stage.makeImmuneTo = stage.makeImmuneTo.Concat(makeImmuneTo).ToList();
                }
                else
                {
                    stage.makeImmuneTo = makeImmuneTo;
                }
            }

            if (capMods != null)
            {
                if (stage.capMods != null)
                {
                    for (int i = capMods.Count - 1; i >= 0; i--)
                    {
                        PawnCapacityModifier ourCap = capMods[i];
                        for (int j = stage.capMods.Count - 1; i >= 0; i--)
                        {
                            PawnCapacityModifier stageCap = stage.capMods[j];

                            if (ourCap.capacity != stageCap.capacity)
                            {
                                continue;
                            }

                            stageCap.offset += ourCap.offset;
                            stageCap.setMax = Math.Min(stageCap.setMax, ourCap.setMax);
                            stageCap.postFactor *= ourCap.postFactor;
                        }
                    }
                }
                else
                {
                    stage.capMods = capMods;
                }
            }

            if (hediffGivers != null)
            {
                if (stage.hediffGivers != null)
                {
                    stage.hediffGivers = stage.hediffGivers.Concat(hediffGivers).ToList();
                }
                else
                {
                    stage.hediffGivers = hediffGivers;
                }
            }

            if (mentalStateGivers != null)
            {
                if (stage.mentalStateGivers != null)
                {
                    stage.mentalStateGivers = stage.mentalStateGivers.Concat(mentalStateGivers).ToList();
                }
                else
                {
                    stage.mentalStateGivers = mentalStateGivers;
                }
            }

            if (statOffsets != null)
            {
                if (stage.statOffsets != null)
                {
                    for (int i = statOffsets.Count - 1; i >= 0; i--)
                    {
                        StatModifier ourMod = statOffsets[i];
                        for (int j = stage.statOffsets.Count - 1; i >= 0; i--)
                        {
                            StatModifier stageMod = stage.statOffsets[j];

                            if (ourMod.stat != stageMod.stat)
                            {
                                continue;
                            }

                            stageMod.value += ourMod.value;
                        }
                    }
                }
                else
                {
                    stage.statOffsets = statOffsets;
                }
            }

            if (statFactors != null)
            {
                if (stage.statFactors != null)
                {
                    for (int i = statFactors.Count - 1; i >= 0; i--)
                    {
                        StatModifier ourMod = statFactors[i];
                        for (int j = stage.statFactors.Count - 1; i >= 0; i--)
                        {
                            StatModifier stageMod = stage.statFactors[j];

                            if (ourMod.stat != stageMod.stat)
                            {
                                continue;
                            }

                            stageMod.value += ourMod.value;
                        }
                    }
                }
                else
                {
                    stage.statFactors = statFactors;
                }
            }

            if (multiplyStatChangesBySeverity != null)
            {
                stage.multiplyStatChangesBySeverity = (bool)multiplyStatChangesBySeverity;
            }

            if (statOffsetEffectMultiplier != null)
            {
                stage.statOffsetEffectMultiplier = statOffsetEffectMultiplier;
            }

            if (statFactorEffectMultiplier != null)
            {
                stage.statFactorEffectMultiplier = statFactorEffectMultiplier;
            }

            if (capacityFactorEffectMultiplier != null)
            {
                stage.capacityFactorEffectMultiplier = capacityFactorEffectMultiplier;
            }

            if (disabledWorkTags != null)
            {
                stage.disabledWorkTags = (WorkTags)disabledWorkTags;
            }

            if (overrideTooltip != null)
            {
                stage.overrideTooltip = overrideTooltip;
            }

            if (extraTooltip != null)
            {
                stage.extraTooltip = extraTooltip;
            }

            stage.partEfficiencyOffset += partEfficiencyOffset;

            if (partIgnoreMissingHP != null)
            {
                stage.partIgnoreMissingHP = (bool)partIgnoreMissingHP;
            }

            return stage;
        }
    }
}
