using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using Verse.Noise;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public static class PawnGroupUtility
    {

        #region ===== Nearby Pawns =====

        public static Dictionary<Pawn, float> NearbyPawnsDistances(IntVec3 cell, Map map, float maxDistance, Faction faction = null, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, Pawn givenPawn = null)
        {
            IReadOnlyList<Pawn> pawns = checkDead ? map.mapPawns.AllPawns : map.mapPawns.AllPawnsSpawned;

            if (faction != null && !hostiles)
            {
                pawns = map.mapPawns.SpawnedPawnsInFaction(faction);
            }

            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            for (int i = pawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = pawns[i];

                if ((pawn.Downed && !checkDowned) || (pawn.Dead && !checkDead))
                {
                    continue;
                }

                if (hostiles)
                {
                    if (givenPawn != null && !pawn.HostileTo(givenPawn))
                    {
                        continue;
                    }

                    if (faction != null && !pawn.HostileTo(faction))
                    {
                        continue;
                    }
                }

                float distance = squaredDistance + 1f;

                if (pawn.Spawned)
                {
                    distance = pawn.Position.DistanceToSquared(cell);
                }
                else if (pawn.Corpse != null)
                {
                    distance = pawn.Corpse.Position.DistanceToSquared(cell);
                }

                if (distance > squaredDistance)
                {
                    continue;
                }

                if (additionalCheck != null && !additionalCheck(pawn, distance))
                {
                    continue;
                }

                result[pawn] = distance;
            }

            return result;
        }

        public static Dictionary<Pawn, float> NearbyPawnsDistances(Pawn pawn, float maxDistance, bool allies = false, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null)
        {
            return NearbyPawnsDistances(pawn.Position, pawn.Map, maxDistance, (allies || hostiles) ? pawn.Faction : null, hostiles, checkDowned, checkDead, additionalCheck, pawn);
        }

        public static List<Pawn> NearbyPawns(IntVec3 cell, Map map, float maxDistance, Faction faction = null, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null)
        {
            return NearbyPawnsDistances(cell, map, maxDistance, faction, hostiles, checkDowned, checkDead, additionalCheck).Keys.ToList();
        }

        public static List<Pawn> NearbyPawns(Pawn pawn, float maxDistance, bool allies = false, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null)
        {            
            return NearbyPawnsDistances(pawn.Position, pawn.Map, maxDistance, (allies || hostiles) ? pawn.Faction : null, hostiles, checkDowned, checkDead, additionalCheck, pawn).Keys.ToList();
        }

        #endregion

        #region ===== Nearby Pawns Thresholds =====

        public static bool NearbyPawnsThreshold(IntVec3 cell, Map map, float maxDistance, int requiredAmount, Faction faction = null, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, Pawn givenPawn = null)
        {
            IReadOnlyList<Pawn> pawns = checkDead ? map.mapPawns.AllPawns : map.mapPawns.AllPawnsSpawned;

            if (faction != null && !hostiles)
            {
                pawns = map.mapPawns.SpawnedPawnsInFaction(faction);
            }

            int count = 0;
            float squaredDistance = maxDistance * maxDistance;

            for (int i = pawns.Count - 1; i >= 0; i--)
            {
                Pawn pawn = pawns[i];

                if ((pawn.Downed && !checkDowned) || (pawn.Dead && !checkDead))
                {
                    continue;
                }

                if (hostiles)
                {
                    if (givenPawn != null && !pawn.HostileTo(givenPawn))
                    {
                        continue;
                    }

                    if (faction != null && !pawn.HostileTo(faction))
                    {
                        continue;
                    }
                }

                float distance = squaredDistance + 1f;

                if (pawn.Spawned)
                {
                    distance = pawn.Position.DistanceToSquared(cell);
                }
                else if (pawn.Corpse != null)
                {
                    distance = pawn.Corpse.Position.DistanceToSquared(cell);
                }

                if (distance > squaredDistance)
                {
                    continue;
                }

                if (additionalCheck != null && !additionalCheck(pawn, distance))
                {
                    continue;
                }

                count += 1;

                if (count == requiredAmount)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool NearbyPawnsThreshold(Pawn pawn, float maxDistance, int requiredAmount, bool allies = false, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null)
        {
            return NearbyPawnsThreshold(pawn.Position, pawn.Map, maxDistance, requiredAmount, (allies || hostiles) ? pawn.Faction : null, hostiles, checkDowned, checkDead, additionalCheck, pawn);
        }

        #endregion

        #region ===== Pawn Grouping =====

        public static List<PawnGroup> GroupPawns(List<Pawn> pawnList, float groupDistance)
        {
            List<PawnGroup> pawnGroups = new List<PawnGroup>();
            float squaredDistance = groupDistance * groupDistance;

            for (int i = pawnList.Count - 1; i >= 0; i--)
            {
                Pawn pawn = pawnList[i];

                if (!pawn.Spawned || pawn.Downed || pawn.Dead)
                {
                    continue;
                }

                PawnGroup groupZero = null;

                for (int j = 0; j < pawnGroups.Count; j++)
                {
                    PawnGroup group = pawnGroups[j];
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
                    PawnGroup newGroup = new PawnGroup(new List<Pawn>() { pawn }, new IntVec3(pawn.Position.x, pawn.Position.y, pawn.Position.z));
                    pawnGroups.Add(newGroup);
                }
            }

            return pawnGroups;
        }

        #endregion

    }

    public class PawnGroup
    {
        public List<Pawn> members;
        public IntVec3 groupCenter;

        public PawnGroup(List<Pawn> newMembers, IntVec3 newGroupCenter)
        {
            members = newMembers;
            groupCenter = newGroupCenter;
        }
    }
}
