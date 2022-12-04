using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class ThinkNodeConditional_UnderGroupCombatPressure : ThinkNode_Conditional
    {
        public float maxThreatDistance = 3f;        // Maximum distance to enemies
        public float maxAllyDistance = 3f;          // At what distance allies are considered to be nearby
        public int soloMinPawns = 3;                // Minimal amount of hostile pawns which is required the condition would be fulfilled
        public int groupPawnMultiplier = 2;         // How much hostile pawns are added to the minimum per ally nearby. With default settings, it will be 3 with 0 allies, 4 with 1, 6 with 2 and etc

        Dictionary<Pawn, bool> cachedGroupThreats = new Dictionary<Pawn, bool>();

        public ThinkNodeConditional_UnderGroupCombatPressure() { }

        protected override bool Satisfied(Pawn pawn)
        {
            if (!pawn.Spawned || pawn.Downed || pawn.Dead)
            {
                return false;
            }

            if (cachedGroupThreats.ContainsKey(pawn))
            {
                bool threatValue = cachedGroupThreats[pawn];
                cachedGroupThreats.Remove(pawn);
                return threatValue;
            }

            List<Pawn> allies = PawnGroupUtility.GetNearbyAllies(pawn, maxAllyDistance);
            int alliesAmount = 1 + allies.Count;
            int hostileThreshold = Math.Max(soloMinPawns, alliesAmount * groupPawnMultiplier);

            TraverseParms tp = TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false, false, false);
            int hostileCount = 0;
            RegionTraverser.BreadthFirstTraverse(pawn.Position, pawn.Map, (Region from, Region to) => to.Allows(tp, false), delegate (Region r)
            {
                List<Thing> possibleHostiles = r.ListerThings.ThingsInGroup(ThingRequestGroup.AttackTarget);
                for (int i = 0; i < possibleHostiles.Count; i++)
                {
                    if (possibleHostiles[i].HostileTo(pawn) && (maxThreatDistance <= 0f || possibleHostiles[i].Position.InHorDistOf(pawn.Position, maxThreatDistance)))
                    {
                        hostileCount += 1;
                        if (hostileCount >= hostileThreshold)
                        {
                            return true;
                        }
                    }
                }
                return true;
            }, 9, RegionType.Set_Passable);

            bool result = hostileCount >= hostileThreshold;

            foreach (Pawn ally in allies)
            {
                cachedGroupThreats[ally] = result;
            }

            return result;
        }

        public override ThinkNode DeepCopy(bool resolve = true)
        {
            ThinkNodeConditional_UnderGroupCombatPressure thinkNodeConditional_UnderGroupCombatPressure = base.DeepCopy(resolve) as ThinkNodeConditional_UnderGroupCombatPressure;
            thinkNodeConditional_UnderGroupCombatPressure.maxThreatDistance = maxThreatDistance;
            thinkNodeConditional_UnderGroupCombatPressure.maxAllyDistance = maxAllyDistance;
            thinkNodeConditional_UnderGroupCombatPressure.soloMinPawns = soloMinPawns;
            thinkNodeConditional_UnderGroupCombatPressure.groupPawnMultiplier = groupPawnMultiplier;
            return thinkNodeConditional_UnderGroupCombatPressure;
        }
    }
}
