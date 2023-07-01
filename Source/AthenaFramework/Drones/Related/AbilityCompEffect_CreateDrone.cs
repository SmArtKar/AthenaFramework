using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class AbilityCompEffect_CreateDrone : CompAbilityEffect
    {
        private new CompProperties_AbilityCreateDrone Props => props as CompProperties_AbilityCreateDrone;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target.Pawn == null)
            {
                return;
            }

            new Drone(target.Pawn, Props.droneDef);
        }
    }

    public class CompProperties_AbilityCreateDrone : CompProperties_AbilityEffect
    {
        public DroneDef droneDef;

        public CompProperties_AbilityCreateDrone()
        {
            this.compClass = typeof(AbilityCompEffect_CreateDrone);
        }
    }
}
