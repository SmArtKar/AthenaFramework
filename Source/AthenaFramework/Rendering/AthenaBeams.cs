﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.Experimental.Rendering.RayTracingAccelerationStructure;

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
    }

    public class BeamInfo : IExposable
    {
        public Thing beamStart;
        public Thing beamEnd;
        public BeamRenderer beam;
        public int ticksLeft = -1;

        public BeamInfo(Thing beamStart, Thing beamEnd, BeamRenderer beam, int ticksLeft = -1)
        {
            this.beamStart = beamStart;
            this.beamEnd = beamEnd;
            this.beam = beam;
            this.ticksLeft = ticksLeft;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref beamStart, "beamStart");
            Scribe_References.Look(ref beamEnd, "beamEnd");
            Scribe_References.Look(ref beam, "beam");
        }
    }

    public class StaticBeamInfo : IExposable
    {
        public Vector3 beamStart;
        public Vector3 beamEnd;
        public BeamRenderer beam;
        public int ticksLeft = -1;

        public StaticBeamInfo(Vector3 beamStart, Vector3 beamEnd, BeamRenderer beam, int ticksLeft = -1)
        {
            this.beamStart = beamStart;
            this.beamEnd = beamEnd;
            this.beam = beam;
            this.ticksLeft = ticksLeft;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref beamStart, "beamStart");
            Scribe_Values.Look(ref beamEnd, "beamEnd");
            Scribe_References.Look(ref beam, "beam");
        }
    }
}
