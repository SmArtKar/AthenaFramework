using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace AthenaFramework
{
    public class BeamActive : Beam
    {
        public Thing beamStart;
        public Thing beamEnd;

        public Vector3 startOffset;
        public Vector3 endOffset;

        public static BeamActive CreateBeam(Thing beamStart, Thing beamEnd, ThingDef beamDef, Vector3 startOffset = new Vector3(), Vector3 endOffset = new Vector3())
        {
            BeamActive beam = ThingMaker.MakeThing(beamDef) as BeamActive;
            GenSpawn.Spawn(beam, (beamStart.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), beamStart.Map);
            beam.beamStart = beamStart;
            beam.beamEnd = beamEnd;
            beam.startOffset = startOffset;
            beam.endOffset = endOffset;
            beam.AdjustBeam(beamStart.DrawPos + startOffset, beamEnd.DrawPos + endOffset);
            return beam;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref beamStart, "beamStart");
            Scribe_References.Look(ref beamEnd, "beamEnd");
        }

        public override void Tick()
        {
            base.Tick();

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
    }
}
