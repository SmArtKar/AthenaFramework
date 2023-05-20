using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class DroneDef : Def
    {
        public Type droneClass;

        public GraphicData graphicData;

        // Drone's stats, like their damage resistances
        public List<StatModifier> statBases;

        // Stat offsets and factors applied to the drone's owner while it is deployed
        public List<StatModifier> ownerStatOffsets;
        public List<StatModifier> ownerStatFactors;

        // Drone's max health. When set to -1, drone is immune to damage
        public float maxHealth = -1f;

        // How fast the drone repairs itself, HP per tick
        public float repairPerTick = 0f;

        // Range at which enemies are considered active combatants
        public float enemyDetectionRange = 9f;

        // How does the drone recover its health
        public DroneRepairType repairType = DroneRepairType.None;

        // How is the drone displayed
        public DroneDisplayType displayType = DroneDisplayType.Shoulder;

        // How likely the drone is to intercept a hit intended for its owner
        public float hitInterceptChance = 0f;

        // When set to 4 values, hit chance will be multiplied by the value of the direction where the hit came from relatively to pawn's facing direction
        // 0 is same direction, 1 is to the right, 2 is from behind, 3 is to the left
        public List<float> directionalHitChanceMultipliers;

        public List<DroneCompProperties> comps;

        // Additional graphic layers with more precise controls. These are drawn on the same layer as drone by default
        public List<DroneGraphicPackage> additionalGraphics = new List<DroneGraphicPackage>();

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
            {
                yield return item;
            }

            if (droneClass == null)
            {
                yield return "abilityClass is null";
            }

            if (label.NullOrEmpty())
            {
                yield return "no label";
            }

            if (statBases != null)
            {
                foreach (StatModifier statBase in statBases)
                {
                    if (statBases.Count((StatModifier st) => st.stat == statBase.stat) > 1)
                    {
                        yield return string.Concat("defines the stat base ", statBase.stat, " more than once.");
                    }
                }
            }

            for (int i = 0; i < comps.Count; i++)
            {
                foreach (string item2 in comps[i].ConfigErrors(this))
                {
                    yield return item2;
                }
            }
        }
    }
}
