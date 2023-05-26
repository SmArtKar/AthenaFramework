using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Hediff_DroneHandler : HediffWithComps
    {
        public HediffStage cachedStage;
        public Drone drone;

        public override string Description
        {
            get
            {
                return base.Description.Translate(drone.Label);
            }
        }

        public override HediffStage CurStage
        {
            get
            {
                if (cachedStage == null)
                {
                    RefreshStage();
                }

                return cachedStage;
            }
        }

        public void RefreshStage()
        {
            cachedStage = new HediffStage();
            cachedStage.becomeVisible = false;
            cachedStage.statOffsets = drone.StatOffsets;
            cachedStage.statFactors = drone.StatFactors;
        }

        public override void Tick()
        {
            base.Tick();
            drone.Tick();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref drone, "drone");
        }

        public override void PostRemoved()
        {
            base.PostRemoved();

            if (drone == null)
            {
                return;
            }

            drone.Recall();
            drone.OnDestroyed();
        }

        public virtual void CleanRemove()
        {
            drone = null;
            pawn.health.RemoveHediff(this);
        }
    }
}
