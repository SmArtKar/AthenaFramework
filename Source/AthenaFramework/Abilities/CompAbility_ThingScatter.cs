using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace AthenaFramework
{
    public class CompAbility_ThingScatter : CompAbilityEffect
    {
        private new CompProperties_AbilityThingScatter Props => props as CompProperties_AbilityThingScatter;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            List<IntVec3> tiles = GenRadial.RadialCellsAround(parent.pawn.Position, Props.radius, false).ToList();

            for (int i = Props.spawnAmount; i > 0; i--)
            {
                IntVec3 tile = tiles.RandomElement();
                tiles.Remove(tile);

                if (Props.thingDef.IsFilth)
                {
                    FilthMaker.TryMakeFilth(tile, parent.pawn.Map, Props.thingDef, Props.thingCount);
                    return;
                }

                Thing thing = ThingMaker.MakeThing(Props.thingDef);
                thing.stackCount = Props.thingCount;
                thing.SetFactionDirect(parent.pawn.Faction);
                GenSpawn.Spawn(thing, tile, parent.pawn.Map);
            }
        }
    }

    public class CompProperties_AbilityThingScatter : CompProperties_AbilityEffect
    {
        public CompProperties_AbilityThingScatter()
        {
            compClass = typeof(CompAbility_ThingScatter);
        }

        public ThingDef thingDef;
        public int spawnAmount;
        // How many things are spawned per tile
        public int thingCount = 1;
        public float radius;
    }
}
