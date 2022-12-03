using System;
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
        public float maxRange;
        public int textureChangeDelay;
        public int sizeTextureAmount = 1;
        public int textureFrameAmount = 1;
    }

    public struct BeamInfo : IExposable
    {
        Thing beamStart;
        Thing beamEnd;
        BeamRenderer beam;

        public BeamInfo(Thing beamStart, Thing beamEnd, BeamRenderer beam)
        {
            this.beamStart = beamStart;
            this.beamEnd = beamEnd;
            this.beam = beam;
        }

        public void ExposeData()
        {
            Scribe_References.Look(ref beamStart, "beamStart");
            Scribe_References.Look(ref beamEnd, "beamEnd");
            Scribe_References.Look(ref beam, "beam");
        }
    }

    public struct StaticBeamInfo : IExposable
    {
        Vector3 beamStart;
        Vector3 beamEnd;
        BeamRenderer beam;

        public StaticBeamInfo(Vector3 beamStart, Vector3 beamEnd, BeamRenderer beam)
        {
            this.beamStart = beamStart;
            this.beamEnd = beamEnd;
            this.beam = beam;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref beamStart, "beamStart");
            Scribe_Values.Look(ref beamEnd, "beamEnd");
            Scribe_References.Look(ref beam, "beam");
        }
    }

    public class BeamRenderer : ThingWithComps
    {
        public Vector3 firstPoint;
        public Vector3 secondPoint;
        public Matrix4x4 matrix;

        public List<List<Material>> materials; //First list is for distance-based textures, second list is for frames
        public Material currentMaterial;

        public int frameAmount = 0;
        public int frameDelayAmount = 0;
        public int currentFrame = 0;
        public int currentFrameTick = 0;

        public int sizeTextureAmount = 0;
        public int currentSize = 1;

        public float maxRange = 25.9f;
        public bool multipleTex = false;

        public BeamRenderer()
        {
            if (def != null)
            {
                setupValues();
            }
        }

        public override void PostPostMake()
        {
            base.PostPostMake();
            setupValues();
        }

        public virtual void setupValues()
        {
            BeamExtension extension = def.GetModExtension<BeamExtension>();
            materials = new List<List<Material>>();

            sizeTextureAmount = extension.sizeTextureAmount;
            frameAmount = extension.textureFrameAmount;
            maxRange = extension.maxRange;

            string texPath = def.graphicData.texPath;
        }
    }
}
