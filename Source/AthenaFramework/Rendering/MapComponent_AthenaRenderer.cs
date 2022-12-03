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

        public void CreateActiveBeam(Thing firstPoint, Thing secondPoint, ThingDef beamDef)
        {
            BeamRenderer beam = ThingMaker.MakeThing(beamDef) as BeamRenderer;
            beam.RenderBeam(firstPoint.DrawPos, secondPoint.DrawPos);
            GenSpawn.Spawn(beam, (firstPoint.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), firstPoint.Map);
            activeBeams.Add(new BeamInfo(firstPoint, secondPoint, beam));
        }

        public void CreateStaticBeam(Thing firstPoint, Thing secondPoint, ThingDef beamDef)
        {
            BeamRenderer beam = ThingMaker.MakeThing(beamDef) as BeamRenderer;
            beam.RenderBeam(firstPoint.DrawPos, secondPoint.DrawPos);
            GenSpawn.Spawn(beam, (firstPoint.DrawPos.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), firstPoint.Map);
            staticBeams.Add(new StaticBeamInfo(firstPoint.DrawPos, secondPoint.DrawPos, beam));
        }

        public void CreateStaticBeam(Vector3 firstPoint, Vector3 secondPoint, ThingDef beamDef, Map map)
        {
            BeamRenderer beam = ThingMaker.MakeThing(beamDef) as BeamRenderer;
            beam.RenderBeam(firstPoint, secondPoint);
            GenSpawn.Spawn(beam, (firstPoint.Yto0() + Vector3.up * beamDef.Altitude).ToIntVec3(), map);
            staticBeams.Add(new StaticBeamInfo(firstPoint, secondPoint, beam));
        }

        public bool DespawnBeam(BeamInfo beamInfo)
        {
            activeBeams.Remove(beamInfo);
            beamInfo.beam.Destroy();
            return true;
        }

        public bool DespawnBeam(StaticBeamInfo beamInfo)
        {
            staticBeams.Remove(beamInfo);
            beamInfo.beam.Destroy();
            return true;
        }

        public bool DespawnBeam(BeamRenderer beam)
        {
            List<BeamInfo> fittingBeams = activeBeams.Where((BeamInfo x) => x.beam == beam).ToList();

            if (fittingBeams.Count > 0)
            {
                BeamInfo activeBeam = fittingBeams[0];
                activeBeams.Remove(activeBeam);
                beam.Destroy();
                return true;
            }

            List<StaticBeamInfo> staticFittingBeams = staticBeams.Where((StaticBeamInfo x) => x.beam == beam).ToList();

            if (staticFittingBeams.Count > 0)
            {
                StaticBeamInfo staticBeam = staticFittingBeams[0];
                staticBeams.Remove(staticBeam);
                beam.Destroy();
                return true;
            }

            return false;
        }
    }
}
