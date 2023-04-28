using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using System.Security.Cryptography;

namespace AthenaFramework
{
    public abstract class GeneGenerationHandler
    {
        public abstract IEnumerable<GeneDef> GenerateDefs(AthenaGeneTemplateDef template);

        public virtual GeneDef CreateDef(AthenaGeneTemplateDef template, Def def, int displayOrder, string iconPath)
        {
            GeneDef geneDef = new GeneDef
            {
                defName = template.defName + "_" + def.defName,
                geneClass = template.geneClass,
                label = template.label.Formatted(def.label),
                iconPath = iconPath,
                description = template.description.Formatted(def.label),
                labelShortAdj = template.labelShortAdj.Formatted(def.label),
                selectionWeight = template.selectionWeight,
                biostatCpx = template.biostatCpx,
                biostatMet = template.biostatMet,
                biostatArc = template.biostatArc,
                displayCategory = template.displayCategory,
                displayOrderInCategory = displayOrder + template.displayOrderOffset,
                minAgeActive = template.minAgeActive,
                modContentPack = template.modContentPack
            };

            if (!template.exclusionTagPrefix.NullOrEmpty())
            {
                geneDef.exclusionTags = new List<string> { template.exclusionTagPrefix + "_" + def.defName };
            }

            return geneDef;
        }
    }
}
