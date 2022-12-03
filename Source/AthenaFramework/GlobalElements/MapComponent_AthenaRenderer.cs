using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class MapComponent_AthenaRenderer : MapComponent
    {
        public List<BeamInfo> activeBeams;

        public MapComponent_AthenaRenderer(Map map) : base(map)
        {

        }


    }

    
}
