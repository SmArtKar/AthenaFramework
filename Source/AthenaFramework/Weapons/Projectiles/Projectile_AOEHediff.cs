using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using Mono.Unix.Native;
using UnityEngine.UIElements;
using Verse.Sound;

namespace AthenaFramework
{
    public class Projectile_AOEHediff : Projectile_Explosive
    {
        public override void Explode()
        {
            Map map = Map;
            IntVec3 pos = Position;
            Destroy();
            if (def.projectile.explosionEffect != null)
            {
                Effecter effecter = def.projectile.explosionEffect.Spawn();
                effecter.Trigger(new TargetInfo(pos, map), new TargetInfo(pos, map));
                effecter.Cleanup();
            }

            if (def.projectile.soundExplode != null)
            {
                def.projectile.soundExplode.PlayOneShot(new TargetInfo(pos, map));
            }

            HediffProjectileExtension extension = def.GetModExtension<HediffProjectileExtension>();

            List<Pawn> pawns = PawnGroupUtility.NearbyPawns(pos, map, extension.radius);

            for (int i = pawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = pawns[i];
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(extension.hediff);

                if (extension.pawnFleck != null)
                {
                    FleckMaker.AttachedOverlay(pawn, extension.pawnFleck, Vector3.zero);
                }

                if (extension.pawnMote != null)
                {
                    MoteMaker.MakeAttachedOverlay(pawn, extension.pawnMote, Vector3.zero);
                }

                if (extension.pawnEffecter != null)
                {
                    Effecter effecter = extension.pawnEffecter.SpawnAttached(pawn, map);
                    effecter.Trigger(new TargetInfo(pawn), new TargetInfo(pawn));
                    effecter.Cleanup();
                }

                if (hediff != null)
                {
                    hediff.Severity += extension.addedSeverity;
                    continue;
                }

                hediff = HediffMaker.MakeHediff(extension.hediff, pawn);
                pawn.health.AddHediff(hediff);
                hediff.Severity = extension.initialSeverity;
            }

            foreach (IntVec3 patternTile in GenRadial.RadialPatternInRadius(extension.radius))
            {
                IntVec3 tile = pos + patternTile;

                if (extension.tileFleck != null)
                {
                    FleckMaker.Static(tile, map, extension.tileFleck);
                }

                if (extension.tileMote != null)
                {
                    MoteMaker.MakeStaticMote(tile, map, extension.tileMote);
                }

                if (extension.tileEffecter != null)
                {
                    Effecter effecter = extension.tileEffecter.Spawn(tile, map);
                    effecter.Trigger(new TargetInfo(tile, map), new TargetInfo(tile, map));
                    effecter.Cleanup();
                }

                if (tile.Walkable(map))
                {
                    if (Rand.Chance(def.projectile.postExplosionSpawnChance))
                    {
                        ThingDef thingDef = tile.GetTerrain(map).IsWater ? (def.projectile.postExplosionSpawnThingDefWater ?? def.projectile.postExplosionSpawnThingDef) : def.projectile.postExplosionSpawnThingDef;
                        TrySpawnExplosionThing(thingDef, tile, map, def.projectile.postExplosionSpawnThingCount);
                    }
                }
            }
        }

        public void TrySpawnExplosionThing(ThingDef thingDef, IntVec3 tile, Map map, int count)
        {
            if (thingDef.IsFilth)
            {
                FilthMaker.TryMakeFilth(tile, map, thingDef, count);
                return;
            }

            Thing thing = ThingMaker.MakeThing(thingDef);
            thing.stackCount = count;
            GenSpawn.Spawn(thing, tile, map);
            thing.TryGetComp<CompReleaseGas>()?.StartRelease();
        }
    }

    public class HediffProjectileExtension : DefModExtension
    {
        public float radius;
        public HediffDef hediff;
        // Severity that the hediff is set to if it didn't exist before
        public float initialSeverity = 1f;
        // Severity that is added to pawns that already had the hediff
        public float addedSeverity;

        public FleckDef tileFleck;
        public ThingDef tileMote;
        public EffecterDef tileEffecter;

        public FleckDef pawnFleck;
        public ThingDef pawnMote;
        public EffecterDef pawnEffecter;
    }
}
