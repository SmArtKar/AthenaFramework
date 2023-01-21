using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Hediff_ModularAddedPart : Hediff_AddedPart
    {
        public HediffStage emptyStage = new HediffStage();

        public override HediffStage CurStage
        {
            get
            {
                HediffStage stage;

                if (def.stages.NullOrEmpty<HediffStage>())
                {
                    stage = emptyStage;
                }
                else
                {
                    stage = def.stages[CurStageIndex];
                }

                for (int i = comps.Count - 1; i >= 0; i--)
                {
                    IStageOverride overrider = comps[i] as IStageOverride;

                    if (overrider != null)
                    {
                        stage = overrider.GetStage(stage);
                    }
                }

                return stage;
            }
        }
    }
}
