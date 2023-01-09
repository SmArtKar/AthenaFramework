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

    public class Beam : ThingWithComps
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

        public bool activeBeam = false;

        public Thing beamStart;
        public Thing beamEnd;

        public Vector3 startOffset;
        public Vector3 endOffset;

        public Beam() { }

        public static Beam CreateActiveBeam(Thing beamStart, Thing beamEnd, ThingDef beamDef, Vector3 startOffset = new Vector3(), Vector3 endOffset = new Vector3())
        {
            Beam beam = ThingMaker.MakeThing(beamDef) as Beam;
            GenSpawn.Spawn(beam, (beamStart.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), beamStart.Map);
            beam.beamStart = beamStart;
            beam.beamEnd = beamEnd;
            beam.startOffset = startOffset;
            beam.endOffset = endOffset;
            beam.activeBeam = true;
            beam.AdjustBeam(beamStart.DrawPos + startOffset, beamEnd.DrawPos + endOffset);
            return beam;
        }

        public static Beam CreateStaticBeam(Thing beamStart, Thing beamEnd, ThingDef beamDef, Vector3 startOffset = new Vector3(), Vector3 endOffset = new Vector3())
        {
            Beam beam = ThingMaker.MakeThing(beamDef) as Beam;
            GenSpawn.Spawn(beam, (beamStart.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), beamStart.Map);
            beam.AdjustBeam(beamStart.DrawPos + startOffset, beamEnd.DrawPos + endOffset);
            return beam;
        }

        public static Beam CreateStaticBeam(Vector3 beamStart, Vector3 beamEnd, ThingDef beamDef, Map map)
        {
            Beam beam = ThingMaker.MakeThing(beamDef) as Beam;
            GenSpawn.Spawn(beam, (beamStart.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), map);
            beam.AdjustBeam(beamStart, beamEnd);
            return beam;
        }

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

                    for (int j = 0; j < frameAmount; j++)
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

            Scribe_References.Look(ref beamStart, "beamStart");
            Scribe_References.Look(ref beamEnd, "beamEnd");
            Scribe_Values.Look(ref startOffset, "startOffset");
            Scribe_Values.Look(ref endOffset, "endOffset");
        }

        public virtual void AdjustBeam(Vector3 firstPoint, Vector3 secondPoint)
        {
            this.firstPoint = firstPoint.Yto0();
            this.secondPoint = secondPoint.Yto0();

            matrix.SetTRS((firstPoint + secondPoint) / 2, Quaternion.LookRotation(secondPoint - firstPoint), new Vector3(def.graphicData.drawSize.x, 1f, (firstPoint - secondPoint).magnitude));
            if (!activeBeam) //Would cause a lot of lag for active beams
            {
                sizeIndex = (int)Math.Min(Math.Ceiling((firstPoint - secondPoint).magnitude / (maxRange / sizeAmount)) - 1, sizeAmount - 1);
            }
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

            if (materials == null)
            {
                return;
            }

            if (frameAmount > 1)
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

                    curMat = materials[sizeIndex][currentFrame];
                }
            }

            if (!activeBeam)
            {
                return;
            }

            if (beamStart == null || beamEnd == null || beamStart.Destroyed || beamEnd.Destroyed || beamStart.Map != beamEnd.Map || beamStart.Map == null)
            {
                Destroy();
                return;
            }

            AdjustBeam(beamStart.DrawPos + startOffset, beamEnd.DrawPos + endOffset);

            if (this.IsHashIntervalTick(20) && sizeAmount > 0)
            {
                if (firstPoint == secondPoint)
                {
                    sizeIndex = 0;
                }
                else
                {
                    float distance = (firstPoint - secondPoint).magnitude;

                    if (distance > maxRange)
                    {
                        for (int i = AllComps.Count - 1; i >= 0; i--)
                        {
                            BeamComp comp = AllComps[i] as BeamComp;

                            if (comp != null)
                            {
                                comp.MaxRangeCut();
                            }
                        }

                        Destroy();
                        return;
                    }

                    sizeIndex = (int)Math.Min(Math.Ceiling(distance / (maxRange / sizeAmount)) - 1, sizeAmount - 1);
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
