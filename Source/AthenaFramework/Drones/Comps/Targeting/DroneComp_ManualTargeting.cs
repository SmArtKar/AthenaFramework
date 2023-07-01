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
    public class DroneComp_ManualTargeting : DroneComp
    {
        public DroneCompProperties_ManualTargeting Props => props as DroneCompProperties_ManualTargeting;

        public virtual void ApplyTarget(LocalTargetInfo target)
        {
            parent.SetTarget(target, Props.targetPriority);
        }

        public override void OnDeployed()
        {
            base.OnDeployed();

            if (Pawn.abilities.GetAbility(Props.targeterAbility) == null)
            {
                Pawn.abilities.GainAbility(Props.targeterAbility);
            }
        }

        public override void OnRecalled()
        {
            base.OnRecalled();

            for (int i = Pawn.health.hediffSet.hediffs.Count; i >= 0; i--)
            {
                Hediff_DroneHandler handler = Pawn.health.hediffSet.hediffs[i] as Hediff_DroneHandler;

                if (handler == null)
                {
                    continue;
                }

                Drone drone = handler.drone;

                DroneComp_ManualTargeting comp = drone.TryGetComp<DroneComp_ManualTargeting>();

                if (comp != null && drone.active)
                {
                    return;
                }
            }

            Pawn.abilities.RemoveAbility(Props.targeterAbility);
        }
    }

    public class DroneCompProperties_ManualTargeting : DroneCompProperties
    {
        public float targetPriority = 100f;
        public AbilityDef targeterAbility;

        public DroneCompProperties_ManualTargeting()
        {
            compClass = typeof(DroneComp_ManualTargeting);
        }
    }
}
