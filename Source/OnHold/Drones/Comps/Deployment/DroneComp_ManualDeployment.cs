using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class DroneComp_ManualDeployment : DroneComp
    {
        private DroneCompProperties_ManualDeployment Props => props as DroneCompProperties_ManualDeployment;

        public Texture2D cachedDeployTex;
        public Texture2D cachedRecallTex;
        public Command_Action deployAction;
        public Command_Action recallAction;

        public override void Initialize(DroneCompProperties props)
        {
            base.Initialize(props);
            cachedDeployTex = ContentFinder<Texture2D>.Get(Props.deployIconTexPath);
            cachedRecallTex = ContentFinder<Texture2D>.Get(Props.recallIconTexPath);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (parent.active)
            {
                if (recallAction == null)
                {
                    recallAction = new Command_Action();
                    recallAction.defaultLabel = Props.recallString.Translate(parent.LabelCap);
                    recallAction.icon = cachedRecallTex;
                    recallAction.action = parent.Recall;
                }

                yield return recallAction;
                yield break;
            }


            if (deployAction == null)
            {
                deployAction = new Command_Action();
                deployAction.defaultLabel = Props.deployString.Translate(parent.LabelCap);
                deployAction.icon = cachedDeployTex;
                deployAction.action = parent.Deploy;
            }

            if (parent.broken)
            {
                if (!deployAction.disabled)
                {
                    deployAction.Disable(Props.brokenReasonString);
                }
            }
            else if (deployAction.disabled)
            {
                deployAction.disabled = false;
            }

            yield return deployAction;
            yield break;
        }
    }

    public class DroneCompProperties_ManualDeployment : DroneCompProperties
    {
        public string deployIconTexPath = "UI/Gizmos/ColorPalette";
        public string recallIconTexPath = "UI/Gizmos/ColorPalette";

        public string deployString = "Deploy {0}";
        public string recallString = "Recall {0}";

        public string brokenReasonString = "{0} is broken";

        public DroneCompProperties_ManualDeployment()
        {
            compClass = typeof(DroneComp_ManualDeployment);
        }
    }
}
