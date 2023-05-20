using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaFramework
{
    public enum DroneDisplayType
    {
        None,           // Drone isn't rendered
        Firefly,        // Drone randomly smoothly flies around the owner, sometimes going behind their back
        Shoulder,       // Drone flies slightly above and to the side of the owner. Multiple drones with this setting will extend into horizontal lines
        ShoulderAbove,  // Same as above, but always above the pawn
        Circling,       // Orbits around the pawn horizontally
        CirclingAbove,  // Circles above the pawns head above them
        Trail,          // Follows the pawn in a trail, multiple drones will extend the tail
        Front           // Always in front of the pawn, smoothly changes its position when the pawn rotates
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
