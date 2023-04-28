using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace AthenaFramework
{
    public class AthenaGeneTemplateDef : Def
    {
        public Type geneClass = typeof(Gene);

        public Type geneHandler;

        public int biostatCpx;

        public int biostatMet;

        public int biostatArc;

        public float minAgeActive;

        public GeneCategoryDef displayCategory;

        public int displayOrderOffset;

        public float selectionWeight = 1f;

        [MustTranslate]
        public string labelShortAdj;

        [NoTranslate]
        public string iconPath;

        [NoTranslate]
        public string exclusionTagPrefix;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string item in base.ConfigErrors())
            {
                yield return item;
            }

            if (!typeof(Gene).IsAssignableFrom(geneClass))
            {
                yield return "geneClass is not Gene or child thereof.";
            }

            if (!typeof(GeneGenerationHandler).IsAssignableFrom(geneHandler))
            {
                yield return "geneHandler is not GeneGenerationHandler or child thereof.";
            }
        }
    }
}
