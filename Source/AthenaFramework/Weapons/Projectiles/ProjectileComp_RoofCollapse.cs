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

        public override void Impact(Thing hitThing, ref bool blockedByShield)
        {
            base.Impact(hitThing, ref blockedByShield);

            List<IntVec3> tileOffsets = GenRadial.RadialPatternInRadius(Props.collaseRange).ToList();
            List<IntVec3> collapseTiles = new List<IntVec3>();

            for (int i = 0; i < tileOffsets.Count; i++)
            {
                IntVec3 tile = tileOffsets[i] + (hitThing != null ? hitThing.Position : Projectile.Position); 

                if (!tile.IsValid || tile.GetRoofHolderOrImpassable(Projectile.Map) != null)
                {
                    continue;
                }

                collapseTiles.Add(tile);
            }

            RoofCollapserImmediate.DropRoofInCells(collapseTiles, Projectile.Map);
        }
    }

    public class CompProperties_ProjectileRoofCollapse : CompProperties
    {
        public CompProperties_ProjectileRoofCollapse()
        {
            this.compClass = typeof(ProjectileComp_RoofCollapse);
        }

        public float collaseRange = 2.9f;
    }
}
