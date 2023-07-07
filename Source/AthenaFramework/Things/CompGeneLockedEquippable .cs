using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace AthenaFramework
{
    public class CompGeneLockedEquippable : ThingComp, IPreventEquip
    {
        public CompProperties_GeneLockedEquippable Props => props as CompProperties_GeneLockedEquippable;

        public override void CompTickRare()
        {
            base.CompTickRare();

            if (!Props.dropWithoutRequirements)
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

            if (Props.requiredXenotype != null && Props.requiredXenotype != pawn.genes.xenotype)
            {
                pawn.equipment.TryDropEquipment(parent, out ThingWithComps thing1, pawn.PositionHeld, false);
                return;
            }

            List<GeneDef> remainingDefs = new List<GeneDef>(Props.requiredGenes);

            for (int i = pawn.genes.GenesListForReading.Count - 1; i >= 0; i--)
            {
                Gene gene = pawn.genes.GenesListForReading[i];

                if (remainingDefs.Contains(gene.def))
                {
                    remainingDefs.Remove(gene.def);
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
            cantReason = null;

            if (Props.requiredXenotype != null && Props.requiredXenotype != pawn.genes.xenotype)
            {
                cantReason = Props.cantReason;
                return true;
            }

            if (Props.blacklistedXenotypes != null && Props.blacklistedXenotypes.Contains(pawn.genes.xenotype))
            {
                cantReason = Props.cantReason;
                return true;
            }

            List<GeneDef> remainingDefs = new List<GeneDef>(Props.requiredGenes);

            for (int i = pawn.genes.GenesListForReading.Count - 1; i >= 0; i--)
            {
                Gene gene = pawn.genes.GenesListForReading[i];

                if (remainingDefs.Contains(gene.def))
                {
                    remainingDefs.Remove(gene.def);
                }

                if (Props.blacklistedGenes != null && Props.blacklistedGenes.Contains(gene.def))
                {
                    cantReason = Props.cantReason;
                    return true;
                }
            }

            if (remainingDefs.Count == 0)
            {
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

    public class CompProperties_GeneLockedEquippable : CompProperties
    {
        public CompProperties_GeneLockedEquippable()
        {
            this.compClass = typeof(CompGeneLockedEquippable);
        }

        // List of genes that a pawn must have to equip this item
        public List<GeneDef> requiredGenes;
        // Xenotype that the pawn must have to equip this item
        public XenotypeDef requiredXenotype;
        // Genes and xenotypes that prevent this item from being equipped
        public List<GeneDef> blacklistedGenes;
        public List<XenotypeDef> blacklistedXenotypes;
        // If equipment should be dropped when the pawn doesn't have required genes/xenotype
        public bool dropWithoutRequirements = false;
        // Text that's displayed when required genes/xenotyopes are missing
        public string cantReason = "Incompatible body structure";
    }
}
