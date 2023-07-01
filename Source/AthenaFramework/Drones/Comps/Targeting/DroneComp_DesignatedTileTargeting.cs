using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class DroneComp_DesignatedTileTargeting : DroneComp
    {
        private DroneCompProperties_DesignatedTileTargeting Props => props as DroneCompProperties_DesignatedTileTargeting;

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (!Pawn.IsHashIntervalTick(Props.searchInterval))
            {
                return;
            }

            List<IntVec3> radialTiles = GenRadial.RadialCellsAround(Pawn.Position, Props.rangeOverride ?? parent.EnemyDetectionRange, false).ToList();

            for (int i = radialTiles.Count - 1; i >= 0; i--)
            {
                IntVec3 tile = radialTiles[i];

                if (Props.requireLineOfSight && !GenSight.LineOfSight(parent.CurrentPosition, tile, Pawn.Map, true))
                {
                    continue;
                }

                if (Pawn.Map.designationManager.DesignationAt(tile, Props.designationDef) != null)
                {
                    parent.SetTarget(tile, Props.targetPriority);
                    return;
                }
            }
        }
    }

    public class DroneCompProperties_DesignatedTileTargeting : DroneCompProperties
    {
        public float targetPriority = 10f;
        public float? rangeOverride;
        public bool requireLineOfSight = true;

        public DesignationDef designationDef = DesignationDefOf.Mine;

        // How often (in ticks) will the comp try to find a new target

        public int searchInterval = 15;

        public DroneCompProperties_DesignatedTileTargeting()
        {
            compClass = typeof(DroneComp_DesignatedTileTargeting);
        }
    }
}
