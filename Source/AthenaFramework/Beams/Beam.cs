using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace AthenaFramework
{
    [Obsolete]
    public class MapComponent_AthenaRenderer : MapComponent
    {
        public MapComponent_AthenaRenderer(Map map) : base(map) { }
    }

    public abstract class Beam : ThingWithComps
    {
        public Vector3 firstPoint;
        public Vector3 secondPoint;
        public Matrix4x4 matrix;

        public List<List<Material>> materials;
        public Material curMat;

        public int frameAmount = 0;
        public int frameDelayAmount = 0;

        public int currentFrame = 0;
        public int currentFrameTick = 0;

        public int sizeIndex = 0;
        public int sizeAmount = 0;
        public float maxRange = 0;

        public int ticksLeft = -1;
        public int fadeoutTicks = -1;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            SetupValues();
        }

        public virtual void SetupValues() 
        {
            BeamExtension extension = def.GetModExtension<BeamExtension>();

            frameAmount = extension.textureFrameAmount;
            frameDelayAmount = extension.textureChangeDelay;

            ticksLeft = extension.beamDuration;
            fadeoutTicks = extension.fadeoutDuration;

            maxRange = extension.maxRange;
            sizeAmount = extension.sizeTextureAmount;

            LongEventHandler.ExecuteWhenFinished(delegate
            {
                materials = new List<List<Material>>();
                string texPath = def.graphicData.texPath;

                for (int i = 0; i < sizeAmount; i++)
                {
                    List<Material> sizeMaterials = new List<Material>();

                    for (int j = 0; j < sizeAmount; j++)
                    {
                        sizeMaterials.Add(MaterialPool.MatFrom(texPath + (sizeAmount > 1 ? ((char)(i + 65)).ToString() : "") + (frameAmount > 1 ? (j + 1) : ""), ShaderDatabase.MoteGlow));
                    }

                    materials.Add(sizeMaterials);
                }

                curMat = materials[0][0];

                for (int i = AllComps.Count - 1; i >= 0; i--)
                {
                    BeamComp comp = AllComps[i] as BeamComp;

                    if (comp != null)
                    {
                        comp.PostTextureSetup();
                    }
                }
            });
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref firstPoint, "firstPoint");
            Scribe_Values.Look(ref secondPoint, "secondPoint");
            Scribe_Values.Look(ref sizeIndex, "sizeIndex");
        }

        public virtual void AdjustBeam(Vector3 firstPoint, Vector3 secondPoint)
        {
            this.firstPoint = firstPoint.Yto0();
            this.secondPoint = secondPoint.Yto0();

            matrix.SetTRS((firstPoint + secondPoint) / 2, Quaternion.LookRotation(secondPoint - firstPoint), new Vector3(def.graphicData.drawSize.x, 1f, (firstPoint - secondPoint).magnitude));
        }

        public override void Draw()
        {
            if (curMat == null)
            {
                return;
            }

            if (fadeoutTicks > 0 && ticksLeft > 0 && ticksLeft <= fadeoutTicks)
            {
                Color color = curMat.color;
                float alpha = ticksLeft / (fadeoutTicks + 1f);
                color.a = alpha;
                curMat.SetColor(ShaderPropertyIDs.Color, color);
            }

            Graphics.DrawMesh(MeshPool.plane10, matrix, curMat, 0);

            for (int i = AllComps.Count - 1; i >= 0; i--)
            {
                BeamComp comp = AllComps[i] as BeamComp;

                if (comp != null)
                {
                    comp.PostDraw();
                }
            }
        }

        public override void Tick()
        {
            base.Tick();

            if (ticksLeft > 0)
            {
                ticksLeft -= 1;
                if (ticksLeft == 0)
                {
                    Destroy();
                    return;
                }
            }

            if (materials == null || frameAmount <= 1)
            {
                return;
            }

            currentFrameTick++;
            if (currentFrameTick > frameDelayAmount)
            {
                currentFrameTick = 0;
                currentFrame++;

                if (currentFrame >= frameAmount)
                {
                    currentFrame = 0;
                }

                curMat = materials[sizeIndex][currentFrame];
            }
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            for (int i = AllComps.Count - 1; i >= 0; i--)
            {
                BeamComp comp = AllComps[i] as BeamComp;

                if (comp != null)
                {
                    comp.PreDestroy();
                }
            }

            base.Destroy(mode);
        }
    }
}
