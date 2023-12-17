using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace AthenaFramework
{
    public class ProjectileComp_RoofCollapse : ProjectileComp
    {
        private CompProperties_ProjectileRoofCollapse Props => props as CompProperties_ProjectileRoofCollapse;

        public Thing storedHitThing;

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            if (storedHitThing == null)
            {
                return;
            }

            List<IntVec3> tileOffsets = GenRadial.RadialPatternInRadius(Props.collapseRange).ToList();
            List<IntVec3> collapseTiles = new List<IntVec3>();

            for (int i = 0; i < tileOffsets.Count; i++)
            {
                IntVec3 tile = tileOffsets[i] + (storedHitThing != null ? storedHitThing.Position : Projectile.Position);

                if (!tile.IsValid || tile.GetRoofHolderOrImpassable(Projectile.Map) != null)
                {
                    continue;
                }

                collapseTiles.Add(tile);
            }

            RoofCollapserImmediate.DropRoofInCells(collapseTiles, Projectile.Map);
        }

        public override void Impact(Thing hitThing, ref bool blockedByShield)
        {
            base.Impact(hitThing, ref blockedByShield);
            storedHitThing = hitThing;
        }
    }

    public class CompProperties_ProjectileRoofCollapse : CompProperties
    {
        public CompProperties_ProjectileRoofCollapse()
        {
            this.compClass = typeof(ProjectileComp_RoofCollapse);
        }

        public float collapseRange = 2.9f;
    }
}
