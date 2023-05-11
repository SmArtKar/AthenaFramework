using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class ProjectileComp_ThingSpawner : ProjectileComp
    {
        public CompProperties_ProjectileThingSpawner Props => props as CompProperties_ProjectileThingSpawner;
        public IntVec3 lastPosition;

        public override void Impact(Thing hitThing, ref bool blockedByShield)
        {
            IntVec3 tile = Projectile.Position;
            Map map = Projectile.Map;
            base.Impact(hitThing, ref blockedByShield);

            if (hitThing != null)
            {
                GenSpawn.Spawn(Props.spawnedDef, Props.previousTile ? lastPosition : hitThing.Position, map);
            }
            else
            {
                GenSpawn.Spawn(Props.spawnedDef, tile, map);
            }
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Projectile.Position != Projectile.DestinationCell)
            {
                lastPosition = Projectile.Position;
            }
        }
    }

    public class CompProperties_ProjectileThingSpawner : CompProperties
    {
        public CompProperties_ProjectileThingSpawner()
        {
            this.compClass = typeof(ProjectileComp_ThingSpawner);
        }

        public ThingDef spawnedDef;
        // When set to true, object will spawn on the last tile before impact. Prevents objects from being spawned in walls.
        public bool previousTile = true;
    }
}
