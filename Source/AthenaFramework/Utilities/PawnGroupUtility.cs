using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public static class PawnGroupUtility
    {
        public static Dictionary<Pawn, float> GetNearbyAlliesWithDistances(Pawn pawn, float maxDistance)
        {
            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn ally in pawn.MapHeld.mapPawns.PawnsInFaction(pawn.Faction))
            {
                if (pawn == ally || !ally.Spawned || ally.Downed || ally.Dead)
                {
                    continue;
                }

                float allyDistance = ally.Position.DistanceToSquared(pawn.Position);
                if (allyDistance <= squaredDistance)
                {
                    result[ally] = allyDistance;
                }
            }

            return result;
        }

        public static List<Pawn> GetNearbyAllies(Pawn pawn, float maxDistance)
        {
            return GetNearbyAlliesWithDistances(pawn, maxDistance).Keys.ToList();
        }

        public static Dictionary<Pawn, float> GetNearbyHostilesWithDistances(Pawn pawn, float maxDistance)
        {
            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn potentialEnemy in pawn.MapHeld.mapPawns.AllPawnsSpawned)
            {
                if (pawn == potentialEnemy || !potentialEnemy.Spawned || potentialEnemy.Downed || potentialEnemy.Dead || !potentialEnemy.Faction.HostileTo(pawn.Faction))
                {
                    continue;
                }

                float enemyDistance = potentialEnemy.Position.DistanceToSquared(pawn.Position);
                if (enemyDistance <= squaredDistance)
                {
                    result[potentialEnemy] = enemyDistance;
                }
            }

            return result;
        }

        public static List<Pawn> GetNearbyHostiles(Pawn pawn, float maxDistance)
        {
            return GetNearbyHostilesWithDistances(pawn, maxDistance).Keys.ToList();
        }

        public static List<PawnGroupup> GroupPawns(List<Pawn> pawnList, float groupDistance)
        {
            List<PawnGroupup> pawnGroups = new List<PawnGroupup>();
            float squaredDistance = groupDistance * groupDistance;

            foreach(Pawn pawn in pawnList)
            {
                if (!pawn.Spawned || pawn.health.Downed || pawn.health.Dead)
                {
                    continue;
                }

                PawnGroupup groupZero = new PawnGroupup();
                bool foundGroup = false;
                for (int j = 0; j < pawnGroups.Count; j++)
                {
                    PawnGroupup group = pawnGroups[j];
                    if (group.groupCenter.DistanceToSquared(pawn.Position) > squaredDistance)
                    {
                        continue;
                    }

                    if (!foundGroup)
                    {
                        groupZero = group;
                        groupZero.members.Add(pawn);
                        groupZero.groupCenter = ((groupZero.groupCenter * groupZero.members.Count) + pawn.Position) * (1 / (groupZero.members.Count + 1));
                    }
                    else
                    {
                        groupZero.groupCenter = ((groupZero.groupCenter * groupZero.members.Count) + (group.groupCenter * group.members.Count)) * (1 / (groupZero.members.Count + group.members.Count));
                        groupZero.members = groupZero.members.Concat(group.members).ToList();
                        pawnGroups.Remove(group);
                        j--;
                    }
                }

                if (!foundGroup)
                {
                    PawnGroupup newGroup = new PawnGroupup(new List<Pawn>() { pawn }, new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z));
                    pawnGroups.Add(newGroup);
                }
            }

            return pawnGroups;
        }
    }

    public struct PawnGroupup
    {
        public List<Pawn> members;
        public IntVec3 groupCenter;

        public PawnGroupup(List<Pawn> newMembers, IntVec3 newGroupCenter)
        {
            members = newMembers;
            groupCenter = newGroupCenter;
        }
    }
}
