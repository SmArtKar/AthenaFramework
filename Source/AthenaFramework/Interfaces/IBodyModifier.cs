using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;

namespace AthenaFramework
{
    public interface IBodyModifier
    {
        public abstract bool CustomApparelTexture(BodyTypeDef bodyType, Apparel apparel, ref ApparelGraphicRecord rec);

        public abstract bool HideBody { get; }

        public abstract bool HideHead { get; }

        public abstract bool HideHair { get; }

        public abstract bool HideFur { get; }

        public abstract bool CustomBodytype(ref BodyTypeDef bodyType);

        public abstract bool CustomHeadtype(ref HeadTypeDef headType);

        public abstract void FurGraphic(ref Graphic furGraphic);

        // Must be added to AthenaCache.bodyCache to work
        // AthenaCache.AddCache(this, AthenaCache.bodyCache, pawn.thingIDNumber)
    }
}
