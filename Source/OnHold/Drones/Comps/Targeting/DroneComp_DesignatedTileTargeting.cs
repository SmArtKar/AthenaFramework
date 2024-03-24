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

        public override bool RecacheTarget(out LocalTargetInfo newTarget, out float newPriority, float rangeOverride = -1f, List<LocalTargetInfo> blacklist = null)
        {
            base.RecacheTarget(out newTarget, out newPriority);

            List<IntVec3> radialTiles = GenRadial.RadialCellsAround(parent.CurrentPosition, rangeOverride != -1f ? rangeOverride : Props.rangeOverride ?? parent.EnemyDetectionRange, false).ToList();

            for (int i = radialTiles.Count - 1; i >= 0; i--)
            {
                IntVec3 tile = radialTiles[i];

                if (blacklist.Contains(tile))
                {
                    continue;
                }

                if (Props.requireLineOfSight && !GenSight.LineOfSight(parent.CurrentPosition, tile, Pawn.Map, true))
                {
                    continue;
                }

                if (Pawn.Map.designationManager.DesignationAt(tile, Props.designationDef) != null)
                {
                    newTarget = tile;
                    newPriority = Props.targetPriority;

                    return true;
                }
            }

            return false;
        }
    }

    public class DroneCompProperties_DesignatedTileTargeting : DroneCompProperties
    {
        public float targetPriority = 10f;
        public float? rangeOverride;
        public bool requireLineOfSight = true;

        public DesignationDef designationDef = DesignationDefOf.Mine;

        public DroneCompProperties_DesignatedTileTargeting()
        {
            compClass = typeof(DroneComp_DesignatedTileTargeting);
        }
    }
}
