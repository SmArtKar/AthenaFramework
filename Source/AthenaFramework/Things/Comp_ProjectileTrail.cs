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

    public class Comp_ProjectileTrail : ThingComp
    {
        private CompProperties_ProjectileTrail Props => props as CompProperties_ProjectileTrail;
        private Projectile projectile => parent as Projectile;

        public Effecter attachedEffecter;
        public MoteAttached attachedMote;
        public MoteDualAttached dualMote;
        public Sustainer sustainer;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref attachedMote, "attachedMote");
            Scribe_References.Look(ref dualMote, "dualMote");
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Props.attachedMote != null)
            {
                if (attachedMote == null || attachedMote.Destroyed)
                {
                    attachedMote = MoteMaker.MakeAttachedOverlay(parent, Props.attachedMote, projectile.ExactRotation * Props.attachedMoteOffset) as MoteAttached;
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

            if (Props.dualMote != null && projectile.Launcher != null)
            {
                if (dualMote == null || dualMote.Destroyed)
                {
                    dualMote = MoteMaker.MakeInteractionOverlay(Props.attachedMote, projectile, projectile.Launcher, projectile.ExactRotation * Props.dualMoteOffsetA, Props.dualMoteOffsetB);
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
                if (Rand.Chance(Props.effectSpawnCurve.Evaluate(projectile.DistanceCoveredFraction)))
                {
                    FleckMaker.Static(projectile.DrawPos, projectile.Map, Props.trailFleck);
                }
            }

            if (Props.trailMote != null)
            {
                if (Rand.Chance(Props.effectSpawnCurve.Evaluate(projectile.DistanceCoveredFraction)))
                {
                    MoteMaker.MakeStaticMote(projectile.DrawPos, projectile.Map, Props.trailMote);
                }
            }

            if (Props.trailEffecter != null)
            {
                if (Rand.Chance(Props.effectSpawnCurve.Evaluate(projectile.DistanceCoveredFraction)))
                {
                    Props.trailEffecter.Spawn(projectile, projectile.Map).Cleanup();
                }
            }
        }
    }

    public class CompProperties_ProjectileTrail : CompProperties
    {
        public CompProperties_ProjectileTrail()
        {
            this.compClass = typeof(Comp_ProjectileTrail);
        }

        // Def for flecks that will be continiously spawned
        public FleckDef trailFleck;
        // Def for motes that will be continiously spawned
        public ThingDef trailMote;
        // Def for effecters that will be continiously spawned
        public EffecterDef trailEffecter;
        // Def for an effecter that will be attached to the projectile
        public EffecterDef projectileEffecter;
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
