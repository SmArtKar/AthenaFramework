using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class AngularShotgunExtension : DefModExtension
    {
        // Amount of pellets that your shotgun fires
        public int pelletCount;
        // Angle betweet fired pellets
        public float pelletAngle;
        // Chance for a pellet to hit a downed target when passing through a tile with one
        public float downedHitChance = 0.20f;
    }
}
