using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class HediffComp_HediffRestorer : HediffComp
    {
        public HediffWithComps hediffToRestore;
        public int ticksToRemove = -1;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (hediffToRestore == null || Pawn == null || parent.Part == null)
            {
                return;
            }

            Pawn.health.AddHediff(hediffToRestore, parent.Part, null, null);
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Deep.Look(ref hediffToRestore, "hediffToRestore");
            Scribe_Values.Look(ref ticksToRemove, "ticksToRemove");
        }

        public override void Notify_PawnPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.Notify_PawnPostApplyDamage(dinfo, totalDamageDealt);

            for (int i = hediffToRestore.comps.Count - 1; i >= 0; i--)
            {
                HediffComp_DisableOnDamage comp = hediffToRestore.comps[i] as HediffComp_DisableOnDamage;

                if (comp.ShouldIncreaseDuration)
                {
                    if (comp.AccumulateDuration)
                    {
                        ticksToRemove += comp.GetDisabledDuration(dinfo, totalDamageDealt);
                    }
                    else
                    {
                        ticksToRemove = Math.Max(ticksToRemove, comp.GetDisabledDuration(dinfo, totalDamageDealt));
                    }
                }
            }
        }

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (ticksToRemove > 0)
            {
                ticksToRemove--;

                if (ticksToRemove <= 0)
                {
                    Pawn.health.RemoveHediff(parent);
                    return;
                }
            }

            if (!Pawn.IsHashIntervalTick(60))
            {
                return;
            }

            bool canReenable = true;

            for (int i = hediffToRestore.comps.Count - 1; i >= 0; i--)
            {
                HediffComp_PerquisiteHediff comp = hediffToRestore.comps[i] as HediffComp_PerquisiteHediff;

                if (!comp.ShouldReenable(Pawn))
                {
                    canReenable = false;
                    break;
                }
            }

            if (canReenable)
            {
                Pawn.health.RemoveHediff(parent);
            }
        }

        public override string CompLabelInBracketsExtra
        {
            get
            {
                if (ticksToRemove > 0)
                {
                    return Mathf.RoundToInt(ticksToRemove / 2500) + "LetterHour".Translate();
                }

                return null;
            }
        }

        public override string CompTipStringExtra
        {
            get
            {
                if (ticksToRemove > 0)
                {
                    return ((ticksToRemove / 2500).ToString("0.0")) + " hours left";
                }
                return null;
            }
        }
    }

    public class HediffCompProperties_HediffRestorer : HediffCompProperties
    {
        public HediffCompProperties_HediffRestorer()
        {
            this.compClass = typeof(HediffComp_HediffRestorer);
        }
    }
}
