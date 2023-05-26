using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaFramework
{
    public enum DroneDisplayType
    {
        Trail,          // Follows the pawn in a trail, multiple drones will extend the tail
        Front           // Always in front of the pawn, smoothly changes its position when the pawn rotates. Multiple drones form a triangle structure
    }

    public enum DroneRepairType
    {
        None,                // There's no way for the drone to recover health
        Passive,             // Drone passively recovers health
        OutOfCombat,         // Drone recovers health when the owner isn't in active combat
        Recalled,            // Drone recovers health when it's recalled
        RecalledOutOfCombat  // Drone recovers health when it's recalled AND the owner is out of combat
    }
}
