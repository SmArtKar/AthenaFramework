using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class ProjectileComp_Trail : ProjectileComp
    {
        private CompProperties_ProjectileTrail Props => props as CompProperties_ProjectileTrail;

        public Effecter attachedEffecter;
        public MoteAttached attachedMote;
        public MoteDualAttached dualMote;
        public Sustainer sustainer;
        public Beam beam;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref attachedMote, "attachedMote");
            Scribe_References.Look(ref dualMote, "dualMote");
            Scribe_References.Look(ref beam, "beam");
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Props.attachedMote != null)
            {
                if (attachedMote == null || attachedMote.Destroyed)
                {
                    attachedMote = MoteMaker.MakeAttachedOverlay(parent, Props.attachedMote, Projectile.ExactRotation * Props.attachedMoteOffset) as MoteAttached;
                }

                attachedMote.Maintain();
            }

            if (Props.projectileEffecter != null)
            {
                if (attachedEffecter == null)
                {
                    attachedEffecter = Props.projectileEffecter.SpawnAttached(parent, parent.Map);
                }

                attachedEffecter.EffectTick(parent, parent);
            }

            if (Props.dualMote != null && Projectile.Launcher != null)
            {
                if (dualMote == null || dualMote.Destroyed)
                {
                    dualMote = MoteMaker.MakeInteractionOverlay(Props.attachedMote, Projectile, Projectile.Launcher, Projectile.ExactRotation * Props.dualMoteOffsetA, Props.dualMoteOffsetB);
                }
                
                dualMote.Maintain();
            }

            if (Props.sustainer != null)
            {
                if (sustainer == null)
                {
                    sustainer = Props.sustainer.TrySpawnSustainer(SoundInfo.InMap(parent, MaintenanceType.PerTick));
                }

                sustainer.Maintain();
            }

            if (Props.trailFleck != null)
            {
                if (Rand.Chance(Props.effectSpawnCurve.Evaluate(Projectile.DistanceCoveredFraction)))
                {
                    FleckMaker.Static(Projectile.DrawPos, Projectile.Map, Props.trailFleck);
                }
            }

            if (Props.trailMote != null)
            {
                if (Rand.Chance(Props.effectSpawnCurve.Evaluate(Projectile.DistanceCoveredFraction)))
                {
                    MoteMaker.MakeStaticMote(Projectile.DrawPos, Projectile.Map, Props.trailMote);
                }
            }

            if (Props.trailEffecter != null)
            {
                if (Rand.Chance(Props.effectSpawnCurve.Evaluate(Projectile.DistanceCoveredFraction)))
                {
                    Props.trailEffecter.Spawn(Projectile, Projectile.Map).Cleanup();
                }
            }
        }

        public override void Launch(Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            base.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);

            beam = Beam.CreateActiveBeam(launcher, parent, Props.beamDef, origin - launcher.DrawPos);
        }

        public override void Impact(Thing hitThing, ref bool blockedByShield)
        {
            base.Impact(hitThing, ref blockedByShield);

            beam.AdjustBeam(Projectile.Launcher.DrawPos + beam.startOffset, parent.DrawPos);
        }
    }

    public class CompProperties_ProjectileTrail : CompProperties
    {
        public CompProperties_ProjectileTrail()
        {
            this.compClass = typeof(ProjectileComp_Trail);
        }

        // Def for flecks that will be continiously spawned
        public FleckDef trailFleck;
        // Def for motes that will be continiously spawned
        public ThingDef trailMote;
        // Def for effecters that will be continiously spawned
        public EffecterDef trailEffecter;
        // Def for an effecter that will be attached to the projectile
        public EffecterDef projectileEffecter;
        // Def for an Athena beam
        public ThingDef beamDef;
        // Chance curve for spawning effects
        public SimpleCurve effectSpawnCurve = new SimpleCurve
        {
            {
                new CurvePoint(0f, 0.5f),
                true
            },
            {
                new CurvePoint(1f, 0.5f),
                true
            }
        };
        // Def for a mote that will be attached to the projectile
        public ThingDef attachedMote;
        // Offset for the mote above. Rotates with the projectile
        public Vector3 attachedMoteOffset;
        // Def for a dual-sided mote that will be attached to the firer and projectile
        public ThingDef dualMote;
        // Offsets for the mote above. Offset A is for the projectile side and rotates with the projectile, while offset B is for the firer and doesn't rotate
        public Vector3 dualMoteOffsetA;
        public Vector3 dualMoteOffsetB;
        // Def for a sustainer that will be attached to the projectile
        public SoundDef sustainer;
    }
}
