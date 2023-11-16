using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class CompPrerequisiteEquippable : ThingComp, IPreventEquip
    {
        public CompProperties_PrerequisiteEquippable Props => props as CompProperties_PrerequisiteEquippable;

        public override void CompTick()
        {
            base.CompTick();

            if (!parent.IsHashIntervalTick(180))
            {
                return;
            }

            if (!Props.dropWithoutPrerequisites)
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

            if (!Props.ValidPawn(pawn))
            {
                return;
            }

            pawn.equipment.TryDropEquipment(parent, out ThingWithComps thing, pawn.PositionHeld, false);
        }

        public bool PreventEquip(Pawn pawn, out string cantReason)
        {
            List<HediffDef> remainingDefs = new List<HediffDef>(Props.prerequisites);

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

            AthenaCache.AddCache(this, ref AthenaCache.equipCache, parent.thingIDNumber);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            AthenaCache.RemoveCache(this, AthenaCache.equipCache, parent.thingIDNumber);
        }
    }

    public class CompProperties_PrerequisiteEquippable : CompProperties
    {
        public CompProperties_PrerequisiteEquippable()
        {
            this.compClass = typeof(CompPrerequisiteEquippable);
        }

        // List of prerequisite hediffs that are required for the item to be equipped
        public List<HediffDef> prerequisites;
        // List of equipment that the pawn should have for the item to be equipped
        public List<ThingDef> equipmentPrerequisites;
        // List of genes that the pawn should have for the item to be equipped
        public List<GeneDef> genePrerequisites;
        // If equipment should be dropped without prerequisite hediffs
        public bool dropWithoutPrerequisites = false;
        // Text that's displayed when the pawn is not fitting the criteria
        public string cantReason = "Missing prerequisites.";

        public virtual bool ValidPawn(Pawn pawn)
        {
            if (genePrerequisites != null && genePrerequisites.Count > 0 && pawn.genes == null)
            {
                return false;
            }

            for (int i = genePrerequisites.Count - 1; i >= 0; i--)
            {
                if (!pawn.genes.HasGene(genePrerequisites[i]))
                {
                    return false;
                }
            }

            List<ThingDef> remainingThings = new List<ThingDef>(equipmentPrerequisites);
            List<ThingWithComps> equipment = pawn.equipment.AllEquipmentListForReading;

            for (int i = equipment.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = equipment[i];

                if (remainingThings.Contains(thing.def))
                {
                    remainingThings.Remove(thing.def);
                }
            }

            if (remainingThings.Count > 0)
            {
                return false;
            }

            List<HediffDef> remainingHediffs = new List<HediffDef>(prerequisites);

            for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = pawn.health.hediffSet.hediffs[i];

                if (remainingHediffs.Contains(hediff.def))
                {
                    remainingHediffs.Remove(hediff.def);
                }
            }

            if (remainingHediffs.Count > 0)
            {
                return false;
            }

            return true;
        }
    }
}
