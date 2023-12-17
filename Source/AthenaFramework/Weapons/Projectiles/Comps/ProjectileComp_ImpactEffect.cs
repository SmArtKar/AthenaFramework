using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class ProjectileComp_ImpactEffect : ProjectileComp
    {
        private CompProperties_ProjectileImpactEffect Props => props as CompProperties_ProjectileImpactEffect;

        public override void Impact(Thing hitThing, ref bool blockedByShield)
        {
            base.Impact(hitThing, ref blockedByShield);

            if (Props.fleck != null)
            {
                FleckMaker.Static(Projectile.DrawPos, parent.Map, Props.fleck, 1f);
            }

            if (Props.mote != null)
            {
                MoteMaker.MakeStaticMote(parent.DrawPos, parent.Map, Props.mote, 1f);
            }

            if (Props.effecter != null)
            {
                Effecter effecter = Props.effecter.Spawn(parent.Position, parent.Map, 1f);
                effecter.offset = parent.DrawPos - parent.Position.ToVector3();
                effecter.Cleanup();
            }
        }
    }

    public class CompProperties_ProjectileImpactEffect : CompProperties
    {
        public CompProperties_ProjectileImpactEffect()
        {
            this.compClass = typeof(ProjectileComp_ImpactEffect);
        }

        public FleckDef fleck;
        public ThingDef mote;
        public EffecterDef effecter;
    }
}
