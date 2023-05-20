using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class PawnRegionUtility
    {

        #region ===== Nearby Pawns =====

        public static Dictionary<Pawn, float> NearbyPawnsDistances(IntVec3 cell, Map map, float maxDistance, TraverseParms parms, Faction faction = null, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, int? regionOverride = null, Pawn givenPawn = null)
        {
            Dictionary<Pawn, float> result = new Dictionary<Pawn, float>();
            float squaredDistance = maxDistance * maxDistance;

            RegionTraverser.BreadthFirstTraverse(cell, map, (Region from, Region to) => to.Allows(parms, isDestination: false), delegate (Region reg)
            {
                List<Thing> pawns = reg.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);

                for (int i = pawns.Count - 1; i >= 0; i--)
                {
                    Pawn pawn = pawns[i] as Pawn;

                    if (pawns[i] is Corpse)
                    {
                        pawn = (pawns[i] as Corpse).InnerPawn;
                    }

                    if ((pawn.Downed && !checkDowned) || (pawn.Dead && !checkDead))
                    {
                        continue;
                    }

                    if (faction != null)
                    {
                        if (hostiles)
                        {
                            if (!pawn.HostileTo(faction))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (pawn.Faction != faction)
                            {
                                continue;
                            }
                        }
                    }
                    
                    if (hostiles && givenPawn != null)
                    {
                        if (!pawn.HostileTo(givenPawn))
                        {
                            continue;
                        }
                    }

                    float distance = pawn.Position.DistanceToSquared(cell);

                    if (distance > squaredDistance)
                    {
                        continue;
                    }

                    if (!additionalCheck(pawn, distance))
                    {
                        continue;
                    }

                    result[pawn] = distance;
                }

                if (!checkDead)
                {
                    return false;
                }

                List<Thing> corpses = reg.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse);

                for (int i = corpses.Count - 1; i >= 0; i--)
                {
                    Corpse corpse = corpses[i] as Corpse;
                    Pawn pawn = corpse.InnerPawn;

                    if (faction != null && !hostiles)
                    {
                        if (pawn.Faction != faction)
                        {
                            continue;
                        }
                    }

                    float distance = corpse.Position.DistanceToSquared(cell);

                    if (distance > squaredDistance)
                    {
                        continue;
                    }

                    if (faction != null)
                    {
                        if (hostiles)
                        {
                            if (!pawn.HostileTo(faction))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (pawn.Faction != faction)
                            {
                                continue;
                            }
                        }
                    }

                    if (hostiles && givenPawn != null)
                    {
                        if (!pawn.HostileTo(givenPawn))
                        {
                            continue;
                        }
                    }

                    if (!additionalCheck(pawn, distance))
                    {
                        continue;
                    }

                    result[pawn] = distance;
                }

                return false;

            }, regionOverride ?? (int)(Math.Max(3, Math.Ceiling(maxDistance / 3) + 1) * Math.Max(3, Math.Ceiling(maxDistance / 3) + 1)));

            return result;
        }

        public static Dictionary<Pawn, float> NearbyPawnsDistances(Pawn pawn, float maxDistance, bool allies = false, bool hostiles = false, bool passDoors = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, int? regionOverride = null)
        {
            TraverseParms parms = (passDoors ? TraverseParms.For(TraverseMode.PassDoors) : TraverseParms.For(pawn));
            return NearbyPawnsDistances(pawn.Position, pawn.Map, maxDistance, parms, (allies || hostiles) ? pawn.Faction : null, hostiles, checkDowned, checkDead, additionalCheck, regionOverride, pawn);
        }

        public static List<Pawn> NearbyPawns(IntVec3 cell, Map map, float maxDistance, TraverseParms parms, Faction faction = null, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, int? regionOverride = null)
        {
            return NearbyPawnsDistances(cell, map, maxDistance, parms, faction, hostiles, checkDowned, checkDead, additionalCheck, regionOverride).Keys.ToList();
        }

        public static List<Pawn> NearbyPawns(Pawn pawn, float maxDistance, bool allies = false, bool hostiles = false, bool passDoors = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, int? regionOverride = null)
        {
            TraverseParms parms = (passDoors ? TraverseParms.For(TraverseMode.PassDoors) : TraverseParms.For(pawn));
            return NearbyPawnsDistances(pawn.Position, pawn.Map, maxDistance, parms, (allies || hostiles) ? pawn.Faction : null, hostiles, checkDowned, checkDead, additionalCheck, regionOverride, pawn).Keys.ToList();
        }

        #endregion

        #region ===== Nearby Pawns Thresholds =====

        public static bool NearbyPawnsThreshold(IntVec3 cell, Map map, float maxDistance, int requiredAmount, TraverseParms parms, Faction faction = null, bool hostiles = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, int? regionOverride = null, Pawn givenPawn = null)
        {
            int count = 0;
            float squaredDistance = maxDistance * maxDistance;

            RegionTraverser.BreadthFirstTraverse(cell, map, (Region from, Region to) => to.Allows(parms, isDestination: false), delegate (Region reg)
            {
                List<Thing> pawns = reg.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn);

                for (int i = pawns.Count - 1; i >= 0; i--)
                {
                    Pawn pawn = pawns[i] as Pawn;

                    if (pawns[i] is Corpse)
                    {
                        pawn = (pawns[i] as Corpse).InnerPawn;
                    }

                    if ((pawn.Downed && !checkDowned) || (pawn.Dead && !checkDead))
                    {
                        continue;
                    }
                    
                    if (faction != null)
                    {
                        if (hostiles)
                        {
                            if (!pawn.HostileTo(faction))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (pawn.Faction != faction)
                            {
                                continue;
                            }
                        }
                    }
                    
                    if (hostiles && givenPawn != null)
                    {
                        if (!pawn.HostileTo(givenPawn))
                        {
                            continue;
                        }
                    }

                    float distance = pawn.Position.DistanceToSquared(cell);

                    if (distance > squaredDistance)
                    {
                        continue;
                    }

                    if (!additionalCheck(pawn, distance))
                    {
                        continue;
                    }

                    count += 1;

                    if (count == requiredAmount)
                    {
                        return true;
                    }
                }

                if (!checkDead)
                {
                    return false;
                }

                List<Thing> corpses = reg.ListerThings.ThingsInGroup(ThingRequestGroup.Corpse);

                for (int i = corpses.Count - 1; i >= 0; i--)
                {
                    Corpse corpse = corpses[i] as Corpse;
                    Pawn pawn = corpse.InnerPawn;

                    if (faction != null)
                    {
                        if (hostiles)
                        {
                            if (!pawn.HostileTo(faction))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (pawn.Faction != faction)
                            {
                                continue;
                            }
                        }
                    }

                    if (hostiles && givenPawn != null)
                    {
                        if (!pawn.HostileTo(givenPawn))
                        {
                            continue;
                        }
                    }

                    float distance = corpse.Position.DistanceToSquared(cell);

                    if (distance > squaredDistance)
                    {
                        continue;
                    }

                    if (!additionalCheck(pawn, distance))
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

            }, regionOverride ?? (int)(Math.Max(3, Math.Ceiling(maxDistance / 3) + 1) * Math.Max(3, Math.Ceiling(maxDistance / 3) + 1)));

            return count == requiredAmount;
        }

        public static bool NearbyPawnsThreshold(Pawn pawn, float maxDistance, int requiredAmount, bool allies = false, bool hostiles = false, bool passDoors = false, bool checkDowned = false, bool checkDead = false, Func<Pawn, float, bool> additionalCheck = null, int? regionOverride = null)
        {
            TraverseParms parms = (passDoors ? TraverseParms.For(TraverseMode.PassDoors) : TraverseParms.For(pawn));
            return NearbyPawnsThreshold(pawn.Position, pawn.Map, maxDistance, requiredAmount, parms, (allies || hostiles) ? pawn.Faction : null, hostiles, checkDowned, checkDead, additionalCheck, regionOverride, pawn);
        }

        #endregion

    }
}
