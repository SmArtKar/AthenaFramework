using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace AthenaFramework
{
    public class MapComponent_AthenaRenderer : MapComponent
    {
        public List<BeamInfo> activeBeams;
        public List<StaticBeamInfo> staticBeams;

        private bool tickHappened = false;

        public MapComponent_AthenaRenderer(Map map) : base(map)
        {
            activeBeams = new List<BeamInfo>();
            staticBeams = new List<StaticBeamInfo>();
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            tickHappened = true;
        }

        public override void MapComponentUpdate()
        {
            base.MapComponentUpdate();
            if (!tickHappened)
            {
                return;
            }

            tickHappened = false;
            foreach (BeamInfo beamInfo in activeBeams)
            {
                beamInfo.beam.RenderBeam(beamInfo.beamStart.DrawPos, beamInfo.beamEnd.DrawPos);
            }
        }

        public BeamInfo CreateActiveBeam(Thing firstPoint, Thing secondPoint, ThingDef beamDef)
        {
            BeamRenderer beam = ThingMaker.MakeThing(beamDef) as BeamRenderer;
            beam.RenderBeam(firstPoint.DrawPos, secondPoint.DrawPos);
            GenSpawn.Spawn(beam, (firstPoint.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), firstPoint.Map);
            BeamInfo beamInfo = new BeamInfo(firstPoint, secondPoint, beam);
            activeBeams.Add(beamInfo);
            return beamInfo;
        }

        public StaticBeamInfo CreateStaticBeam(Thing firstPoint, Thing secondPoint, ThingDef beamDef)
        {
            BeamRenderer beam = ThingMaker.MakeThing(beamDef) as BeamRenderer;
            beam.RenderBeam(firstPoint.DrawPos, secondPoint.DrawPos);
            GenSpawn.Spawn(beam, (firstPoint.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), firstPoint.Map);
            StaticBeamInfo beamInfo = new StaticBeamInfo(firstPoint.DrawPos, secondPoint.DrawPos, beam);
            staticBeams.Add(beamInfo);
            return beamInfo;
        }

        public StaticBeamInfo CreateStaticBeam(Vector3 firstPoint, Vector3 secondPoint, ThingDef beamDef, Map map)
        {
            BeamRenderer beam = ThingMaker.MakeThing(beamDef) as BeamRenderer;
            beam.RenderBeam(firstPoint, secondPoint);
            GenSpawn.Spawn(beam, (firstPoint.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), map);
            StaticBeamInfo beamInfo = new StaticBeamInfo(firstPoint, secondPoint, beam);
            staticBeams.Add(beamInfo);
            return beamInfo;
        }

        public bool DestroyBeam(BeamInfo beamInfo)
        {
            beamInfo.beam.PreDestroyBeam();
            activeBeams.Remove(beamInfo);
            beamInfo.beam.Destroy();
            return true;
        }

        public bool DestroyBeam(StaticBeamInfo beamInfo)
        {
            beamInfo.beam.PreDestroyBeam();
            staticBeams.Remove(beamInfo);
            beamInfo.beam.Destroy();
            return true;
        }

        public bool DestroyBeam(BeamRenderer beam)
        {
            List<BeamInfo> fittingBeams = activeBeams.Where((BeamInfo x) => x.beam == beam).ToList();

            if (fittingBeams.Count > 0)
            {
                return DestroyBeam(fittingBeams[0]);
            }

            List<StaticBeamInfo> staticFittingBeams = staticBeams.Where((StaticBeamInfo x) => x.beam == beam).ToList();

            if (staticFittingBeams.Count > 0)
            {
                return DestroyBeam(staticFittingBeams[0]);
            }

            return false;
        }
    }
}
