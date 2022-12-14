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
        public static Dictionary<Pawn, float> GetNearbyAlliesWithDistances(IntVec3 cell, Map map, Faction faction, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn ally in map.mapPawns.PawnsInFaction(faction))
            {
                if (!ally.Spawned || (ally.Downed && !checkDowned) || (ally.Dead && !checkDead))
                {
                    continue;
                }

                float allyDistance = ally.Position.DistanceToSquared(cell);
                if (allyDistance <= squaredDistance)
                {
                    result[ally] = allyDistance;
                }
            }

            return result;
        }

        public static Dictionary<Pawn, float> GetNearbyAlliesWithDistances(Pawn pawn, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyAlliesWithDistances(pawn.Position, pawn.MapHeld, pawn.Faction, maxDistance, checkDowned, checkDead);
        }

        public static List<Pawn> GetNearbyAllies(Pawn pawn, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyAlliesWithDistances(pawn, maxDistance, checkDowned, checkDead).Keys.ToList();
        }

        public static List<Pawn> GetNearbyAllies(IntVec3 cell, Map map, Faction faction, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyAlliesWithDistances(cell, map, faction, maxDistance, checkDowned, checkDead).Keys.ToList();
        }

        public static bool AlliedPawnsNearbyThreshold(IntVec3 cell, Map map, Faction faction, float maxDistance, int alliesAmount, bool checkDowned = false, bool checkDead = false)
        {
            int nearbyAllies = 0;
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn ally in map.mapPawns.PawnsInFaction(faction))
            {
                if (!ally.Spawned || (ally.Downed && !checkDowned) || (ally.Dead && !checkDead))
                {
                    continue;
                }

                if (ally.Position.DistanceToSquared(cell) <= squaredDistance)
                {
                    nearbyAllies++;
                    if (nearbyAllies >= alliesAmount)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool AlliedPawnsNearbyThreshold(Pawn pawn, float maxDistance, int alliesAmount, bool checkDowned = false, bool checkDead = false)
        {
            return AlliedPawnsNearbyThreshold(pawn.Position, pawn.MapHeld, pawn.Faction, maxDistance, alliesAmount, checkDowned, checkDead);
        }

        public static Dictionary<Pawn, float> GetNearbyHostilesWithDistances(IntVec3 cell, Map map, Faction faction, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn potentialEnemy in map.mapPawns.AllPawnsSpawned)
            {
                if (!potentialEnemy.Spawned || (potentialEnemy.Downed && !checkDowned) || (potentialEnemy.Dead && !checkDead) || !potentialEnemy.Faction.HostileTo(faction))
                {
                    continue;
                }

                float enemyDistance = potentialEnemy.Position.DistanceToSquared(cell);
                if (enemyDistance <= squaredDistance)
                {
                    result[potentialEnemy] = enemyDistance;
                }
            }

            return result;
        }

        public static Dictionary<Pawn, float> GetNearbyHostilesWithDistances(Pawn pawn, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyHostilesWithDistances(pawn.Position, pawn.MapHeld, pawn.Faction, maxDistance, checkDowned, checkDead);
        }

        public static List<Pawn> GetNearbyHostiles(Pawn pawn, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyHostilesWithDistances(pawn, maxDistance, checkDowned, checkDead).Keys.ToList();
        }

        public static List<Pawn> GetNearbyHostiles(IntVec3 cell, Map map, Faction faction, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyHostilesWithDistances(cell, map, faction, maxDistance, checkDowned, checkDead).Keys.ToList();
        }

        public static bool HostilePawnsNearbyThreshold(IntVec3 cell, Map map, Faction faction, float maxDistance, int hostilesAmount, bool checkDowned = false, bool checkDead = false)
        {
            int nearbyHostiles = 0;
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn potentialEnemy in map.mapPawns.AllPawnsSpawned)
            {
                if (!potentialEnemy.Spawned || (potentialEnemy.Downed && !checkDowned) || (potentialEnemy.Dead && !checkDead) || !potentialEnemy.Faction.HostileTo(faction))
                {
                    continue;
                }

                if (potentialEnemy.Position.DistanceToSquared(cell) <= squaredDistance)
                {
                    nearbyHostiles++;
                    if (nearbyHostiles >= hostilesAmount)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HostilePawnsNearbyThreshold(Pawn pawn, float maxDistance, int hostilesAmount, bool checkDowned = false, bool checkDead = false)
        {
            return HostilePawnsNearbyThreshold(pawn.Position, pawn.MapHeld, pawn.Faction, maxDistance, hostilesAmount, checkDowned, checkDead);
        }

        public static Dictionary<Pawn, float> GetNearbyPawnsWithDistances(IntVec3 cell, Map map, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.Spawned || (pawn.Downed && !checkDowned) || (pawn.Dead && !checkDead))
                {
                    continue;
                }

                float enemyDistance = pawn.Position.DistanceToSquared(cell);
                if (enemyDistance <= squaredDistance)
                {
                    result[pawn] = enemyDistance;
                }
            }

            return result;
        }

        public static Dictionary<Pawn, float> GetNearbyPawnsWithDistances(Pawn pawn, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyPawnsWithDistances(pawn.Position, pawn.MapHeld, maxDistance, checkDowned, checkDead);
        }

        public static List<Pawn> GetNearbyPawns(Pawn pawn, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyPawnsWithDistances(pawn, maxDistance, checkDowned, checkDead).Keys.ToList();
        }

        public static List<Pawn> GetNearbyPawns(IntVec3 cell, Map map, float maxDistance, bool checkDowned = false, bool checkDead = false)
        {
            return GetNearbyPawnsWithDistances(cell, map, maxDistance, checkDowned, checkDead).Keys.ToList();
        }

        public static bool PawnsNearbyThreshold(IntVec3 cell, Map map, float maxDistance, int pawnAmount, bool checkDowned = false, bool checkDead = false)
        {
            int nearbyPawns = 0;
            float squaredDistance = maxDistance * maxDistance;

            foreach (Pawn pawn in map.mapPawns.AllPawnsSpawned)
            {
                if (!pawn.Spawned || (pawn.Downed && !checkDowned) || (pawn.Dead && !checkDead))
                {
                    continue;
                }

                if (pawn.Position.DistanceToSquared(cell) <= squaredDistance)
                {
                    nearbyPawns++;
                    if (nearbyPawns >= pawnAmount)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool PawnsNearbyThreshold(Pawn pawn, float maxDistance, int pawnAmount, bool checkDowned = false, bool checkDead = false)
        {
            return PawnsNearbyThreshold(pawn.Position, pawn.MapHeld, maxDistance, pawnAmount, checkDowned, checkDead);
        }

        public static List<PawnGroupup> GroupPawns(List<Pawn> pawnList, float groupDistance)
        {
            List<PawnGroupup> pawnGroups = new List<PawnGroupup>();
            float squaredDistance = groupDistance * groupDistance;

            foreach(Pawn pawn in pawnList)
            {
                if (!pawn.Spawned || pawn.Downed || pawn.Dead)
                {
                    continue;
                }

                PawnGroupup groupZero = null;
                for (int j = 0; j < pawnGroups.Count; j++)
                {
                    PawnGroupup group = pawnGroups[j];
                    if (group.groupCenter.DistanceToSquared(pawn.Position) > squaredDistance)
                    {
                        continue;
                    }

                    if (groupZero == null)
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

                if (groupZero == null)
                {
                    PawnGroupup newGroup = new PawnGroupup(new List<Pawn>() { pawn }, new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z));
                    pawnGroups.Add(newGroup);
                }
            }

            return pawnGroups;
        }
    }

    public class PawnGroupup
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
