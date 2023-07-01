using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    internal class CompAbility_DroneTargeting : CompAbilityEffect
    {
        private new CompProperties_AbilityDroneTargeting Props => props as CompProperties_AbilityDroneTargeting;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            for (int i = parent.pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff_DroneHandler hediff = parent.pawn.health.hediffSet.hediffs[i] as Hediff_DroneHandler;

                if (hediff == null)
                {
                    continue;
                }

                Drone drone = hediff.drone;

                for (int j = drone.comps.Count - 1; j >= 0; j++)
                {
                    DroneComp_ManualTargeting comp = drone.comps[j] as DroneComp_ManualTargeting;

                    if (comp != null && comp.Props.targeterAbility == parent.def)
                    {
                        comp.ApplyTarget(target);
                        return;
                    }
                }
            }
        }
    }

    public class CompProperties_AbilityDroneTargeting : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityDroneTargeting()
        {
            this.compClass = typeof(CompAbility_DroneTargeting);
        }
    }
}
