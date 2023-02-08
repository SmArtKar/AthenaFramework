using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public interface IRenderable
    {
        public abstract void DrawAt(Vector3 drawPos, BodyTypeDef bodyType);

        public abstract void RecacheGraphicData();

        // Must be added to AthenaCache.renderCache to work
        // AthenaCache.AddCache(this, AthenaCache.renderCache, pawn.thingIDNumber)
    }
}
