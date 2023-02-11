using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public interface IFloatMenu
    {
        public abstract IEnumerable<FloatMenuOption> ItemFloatMenuOptions(Pawn selPawn);

        public abstract IEnumerable<FloatMenuOption> PawnFloatMenuOptions(ThingWithComps thing);
    }
}
