using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class CompPerquisiteEquippable : ThingComp, IPreventEquip
    {
        public CompProperties_PerquisiteEquippable Props => props as CompProperties_PerquisiteEquippable;

        public override void CompTickRare()
        {
            base.CompTickRare();

            if (!Props.dropWithoutPerquisites)
            {
                return;
            }

            CompEquippable comp = parent.GetComp<CompEquippable>();

            if (comp == null)
            {
                return;
            }

            Pawn pawn = comp.Holder;

            if (pawn == null)
            {
                return;
            }

            List<HediffDef> remainingDefs = new List<HediffDef>(Props.perquisites);

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];

                if (remainingDefs.Contains(hediff.def))
                {
                    remainingDefs.Remove(hediff.def);
                }
            }

            if (remainingDefs.Count == 0)
            {
                return;
            }

            pawn.equipment.TryDropEquipment(parent, out ThingWithComps thing, pawn.PositionHeld, false);
        }

        public bool PreventEquip(Pawn pawn, out string cantReason)
        {
            List<HediffDef> remainingDefs = new List<HediffDef>(Props.perquisites);

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];

                if (remainingDefs.Contains(hediff.def))
                {
                    remainingDefs.Remove(hediff.def);
                }
            }

            if (remainingDefs.Count == 0)
            {
                cantReason = null;
                return false;
            }

            cantReason = Props.cantReason;
            return true;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            AthenaCache.AddCache(this, AthenaCache.equipCache, parent.thingIDNumber);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            AthenaCache.RemoveCache(this, AthenaCache.equipCache, parent.thingIDNumber);
        }
    }

    public class CompProperties_PerquisiteEquippable : CompProperties
    {
        public CompProperties_PerquisiteEquippable()
        {
            this.compClass = typeof(CompPerquisiteEquippable);
        }

        // List of perquisite hediffs that are required for apparel to be equipped
        public List<HediffDef> perquisites;
        // If equipment should be dropped without perquisite hediffs
        public bool dropWithoutPerquisites = false;
        // Text that's displayed when required hediffs are missing
        public string cantReason = "Missing required hediffs";
    }
}
