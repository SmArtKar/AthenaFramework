using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class BeamExtension : DefModExtension
    {
        // Range at which the beam is cut
        public float maxRange;
        // Delay between beam frames
        public int textureChangeDelay;
        // Amount of texture frames
        public int textureFrameAmount = 1;
        // Amount of textures for different beam lengths
        public int sizeTextureAmount = 1;
        // Duration after which beam will be destroyed. Disabled if set to -1
        public int beamDuration = -1;
        // Duration for which the beam fades out before being destroyed. Disabled if set to -1 or if beamDuration is disabled
        public int fadeoutDuration = -1;
    }
}
