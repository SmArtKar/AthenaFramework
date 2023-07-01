using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class GeneLockedRecipeExtension : DefModExtension
    {
        // List of genes that a pawn must have to build this building or use the recipe
        public List<GeneDef> requiredGenes;
        // Xenotype that the pawn must have to build this building or use the recipe
        public XenotypeDef requiredXenotype;
        // Message that's displayed when a pawn without required genes/xenotype tries to use the recipe
        public string cantReason = "Incompatible body structure";

        public bool CanCreate(Pawn pawn)
        {
            if (requiredXenotype != null && requiredXenotype != pawn.genes.xenotype)
            {
                return false;
            }

            List<GeneDef> remainingDefs = new List<GeneDef>(requiredGenes);

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
                return true;
            }

            return false;
        }
    }
}
