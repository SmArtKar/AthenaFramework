using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class BeamStatic : Beam
    {
        public static BeamStatic CreateBeam(Thing beamStart, Thing beamEnd, ThingDef beamDef, Vector3 startOffset = new Vector3(), Vector3 endOffset = new Vector3())
        {
            BeamStatic beam = ThingMaker.MakeThing(beamDef) as BeamStatic;
            GenSpawn.Spawn(beam, (beamStart.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), beamStart.Map);
            beam.AdjustBeam(beamStart.DrawPos + startOffset, beamEnd.DrawPos + endOffset);
            return beam;
        }

        public static BeamStatic CreateBeam(Vector3 beamStart, Vector3 beamEnd, ThingDef beamDef, Map map)
        {
            BeamStatic beam = ThingMaker.MakeThing(beamDef) as BeamStatic;
            GenSpawn.Spawn(beam, (beamStart.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), map);
            beam.AdjustBeam(beamStart, beamEnd);
            return beam;
        }

        public override void AdjustBeam(Vector3 firstPoint, Vector3 secondPoint)
        {
            this.firstPoint = firstPoint.Yto0();
            this.secondPoint = secondPoint.Yto0();

            matrix.SetTRS((firstPoint + secondPoint) / 2, Quaternion.LookRotation(secondPoint - firstPoint), new Vector3(def.graphicData.drawSize.x, 1f, (firstPoint - secondPoint).magnitude));
            sizeIndex = (int)Math.Min(Math.Ceiling((firstPoint - secondPoint).magnitude / (maxRange / sizeAmount)) - 1, sizeAmount - 1);
        }
    }
}
