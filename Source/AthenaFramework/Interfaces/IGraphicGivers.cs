using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AthenaFramework
{
    public interface IHediffGraphicGiver
    {
        public abstract List<HediffGraphicPackage> GetAdditionalGraphics { get; }
    }

    public interface IEquippableGraphicGiver
    {
        public abstract List<ApparelGraphicPackage> GetAdditionalGraphics { get; }
    }
}
