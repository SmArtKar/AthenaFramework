using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
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
        public int currentSize = 0;

        public int ticksLeft = -1;
        public int fadeoutTicks = -1;

        public float maxRange = 25.9f;
        public bool multipleTex = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            setupValues();
        }

        public virtual void setupValues()
        {
            BeamExtension extension = def.GetModExtension<BeamExtension>();
            materials = new List<List<Material>>();

            sizeTextureAmount = extension.sizeTextureAmount;
            frameAmount = extension.textureFrameAmount;
            maxRange = extension.maxRange;
            ticksLeft = extension.beamDuration;

            string texPath = def.graphicData.texPath;

            if (extension.textureFrameAmount == 1 && extension.sizeTextureAmount == 1)
            {
                currentMaterial = MaterialPool.MatFrom(texPath, ShaderDatabase.MoteGlow);
                multipleTex = false;
                return;
            }

            multipleTex = true;

            for (int i = 0; i < sizeTextureAmount; i++)
            {
                List<Material> sizeMaterials = new List<Material>();

                for (int j = 0; j < frameAmount; j++)
                {
                    sizeMaterials.Add(MaterialPool.MatFrom(texPath + (extension.sizeTextureAmount > 1 ? ((char)(i + 65)).ToString() : "") + (extension.textureFrameAmount > 1 ? (j + 1) : ""), ShaderDatabase.MoteGlow));
                }

                materials.Add(sizeMaterials);
            }

            frameDelayAmount = extension.textureChangeDelay;
            currentMaterial = materials[0][0];

            foreach (ThingComp thingComp in AllComps)
            {
                if (!(thingComp is CompBeam))
                {
                    continue;
                }

                CompBeam beamComp = thingComp as CompBeam;
                beamComp.PostValuesSetup();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref firstPoint, "firstPoint");
            Scribe_Values.Look(ref secondPoint, "secondPoint");
        }

        public virtual void RenderBeam(Vector3 firstPoint, Vector3 secondPoint)
        {
            this.firstPoint = firstPoint.Yto0();
            this.secondPoint = secondPoint.Yto0();
            matrix.SetTRS((firstPoint + secondPoint) / 2, Quaternion.LookRotation(secondPoint - firstPoint), new Vector3(def.graphicData.drawSize.x, 1f, (firstPoint - secondPoint).magnitude));
        }

        public override void Draw()
        {
            if (ticksLeft <= fadeoutTicks && fadeoutTicks > 0)
            {
                Color color = currentMaterial.color;
                color.a = ticksLeft / (fadeoutTicks + 1);
            }
            
            Graphics.DrawMesh(MeshPool.plane10, matrix, currentMaterial, 0);
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksLeft > 0)
            {
                ticksLeft -= 1;
                if (ticksLeft == 0)
                {
                    DestroyBeam();
                    return;
                }
            }

            if (!multipleTex)
            {
                return;
            }

            if (frameAmount > 0)
            {
                currentFrameTick++;
                if (currentFrameTick > frameDelayAmount)
                {
                    currentFrameTick = 0;
                    currentFrame++;
                    if (currentFrame >= frameAmount)
                    {
                        currentFrame = 0;
                    }
                    currentMaterial = materials[currentSize][currentFrame];
                }
            }

            if (this.IsHashIntervalTick(20) && sizeTextureAmount > 0)
            {
                if (firstPoint == secondPoint)
                {
                    currentSize = 0;
                }
                else
                {
                    float distance = (firstPoint - secondPoint).magnitude;
                    if (distance > maxRange)
                    {
                        foreach (ThingComp thingComp in AllComps)
                        {
                            if (!(thingComp is CompBeam))
                            {
                                continue;
                            }

                            CompBeam beamComp = thingComp as CompBeam;
                            beamComp.MaxRangeCut();
                        }

                        DestroyBeam();
                        return;
                    }

                    currentSize = (int)Math.Min(Math.Ceiling(distance / (maxRange / sizeTextureAmount)) - 1, sizeTextureAmount - 1);
                }

                currentMaterial = materials[currentSize][currentFrame];
            }
        }

        public virtual void DestroyBeam()
        {
            foreach (ThingComp thingComp in AllComps)
            {
                if (!(thingComp is CompBeam))
                {
                    continue;
                }

                CompBeam beamComp = thingComp as CompBeam;
                beamComp.PreSelfDestroyBeam();
            }

            MapHeld.GetComponent<MapComponent_AthenaRenderer>().DestroyBeam(this);
        }

        public virtual void PreDestroyBeam()
        {
            foreach (ThingComp thingComp in AllComps)
            {
                if (!(thingComp is CompBeam))
                {
                    continue;
                }

                CompBeam beamComp = thingComp as CompBeam;
                beamComp.PreDestroyBeam();
            }
        }
    }
}
