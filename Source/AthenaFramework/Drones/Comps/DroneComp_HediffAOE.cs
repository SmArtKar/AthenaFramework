using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class DroneComp_HediffAOE : DroneComp
    {
        private DroneCompProperties_HediffAOE Props => props as DroneCompProperties_HediffAOE;

        public override void ActiveTick()
        {
            base.ActiveTick();

            if (!Pawn.IsHashIntervalTick(Props.tickFrequency))
            {
                return;
            }

            List<Pawn> pawns = Pawn.Map.mapPawns.AllPawnsSpawned;
            float checkDistance = Props.radius * Props.radius;

            for (int i = pawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = pawns[i];

                if (pawn.Position.DistanceToSquared(Pawn.Position) > checkDistance)
                {
                    continue;
                }

                if ((Props.hostileOnly && !pawn.HostileTo(Pawn)) || (Props.friendlyOnly && pawn.HostileTo(Pawn)))
                {
                    continue;
                }

                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.hediffDef);

                if (hediff == null)
                {
                    hediff = pawn.health.AddHediff(Props.hediffDef);
                    hediff.Severity = Props.initialSeverity;
                    return;
                }

                float addedSeverity = Props.severityPerSecond * Props.tickFrequency / 60;

                if (Props.directStats != null)
                {
                    for (int j = Props.directStats.Count - 1; j >= 0; j--)
                    {
                        addedSeverity *= pawn.GetStatValue(Props.directStats[j]);
                    }
                }

                if (Props.inverseStats != null)
                {
                    for (int j = Props.inverseStats.Count - 1; j >= 0; j--)
                    {
                        addedSeverity /= pawn.GetStatValue(Props.inverseStats[j]);
                    }
                }

                hediff.Severity += addedSeverity;
            }
        }
    }

    public class DroneCompProperties_HediffAOE : DroneCompProperties
    {
        public HediffDef hediffDef;
        public float radius = 4.9f;
        // Severity added per second to those pawns who already have the hediff
        public float severityPerSecond = 0.15f;
        // Severity set for pawns who don't have the hediff
        public float initialSeverity = 1f;
        // severityPerSecond is multiplied by stat value from directStats and divided by stat value from inverseStats
        public List<StatDef> directStats;
        public List<StatDef> inverseStats;
        // Frequency with which radius checks are applied, in ticks
        public int tickFrequency = 15;
        // If only hostiles/friendlies should be affected
        public bool hostileOnly = false;
        public bool friendlyOnly = false;

        public DroneCompProperties_HediffAOE()
        {
            compClass = typeof(DroneComp_HediffAOE);
        }
    }
}
