using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static HarmonyLib.Code;

namespace AthenaFramework
{
    public class DroneComp_MassDeployment : DroneComp_ManualDeployment
    {
        private DroneCompProperties_MassDeployment Props => props as DroneCompProperties_MassDeployment;

        public bool soleFitting = false;
        public bool otherActiveDrones = false;
        public bool otherInactiveDrones = false;

        public override void Tick()
        {
            base.Tick();

            if (!Pawn.IsHashIntervalTick(15))
            {
                return;
            }


            otherActiveDrones = false;
            otherInactiveDrones = false;
            soleFitting = false;

            bool canBecomeFitting = true;

            for (int i = Pawn.health.hediffSet.hediffs.Count; i >= 0; i--)
            {
                Hediff_DroneHandler handler = Pawn.health.hediffSet.hediffs[i] as Hediff_DroneHandler;

                if (handler == null)
                {
                    continue;
                }

                Drone drone = handler.drone;

                if (Props.onlySameType && drone.def != parent.def)
                {
                    continue;
                }

                DroneComp_MassDeployment deployComp = drone.TryGetComp<DroneComp_MassDeployment>();

                if (deployComp == null)
                {
                    continue;
                }

                if (!Props.onlySameType && (deployComp.props as DroneCompProperties_MassDeployment).onlySameType)
                {
                    continue;
                }

                if (drone.active)
                {
                    otherActiveDrones = true;
                }
                else
                {
                    otherInactiveDrones = true;
                }

                if (deployComp.soleFitting)
                {
                    canBecomeFitting = false;
                }
            }

            if (canBecomeFitting)
            {
                soleFitting = true;
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!soleFitting)
            {
                yield break;
            }

            if (parent.active || otherActiveDrones)
            {
                if (recallAction == null)
                {
                    recallAction = new Command_Action();
                    recallAction.defaultLabel = Props.recallString.Translate(parent.LabelCap);
                    recallAction.icon = cachedRecallTex;
                    recallAction.action = delegate ()
                    {
                        for (int i = Pawn.health.hediffSet.hediffs.Count; i >= 0; i--)
                        {
                            Hediff_DroneHandler handler = Pawn.health.hediffSet.hediffs[i] as Hediff_DroneHandler;

                            if (handler == null)
                            {
                                continue;
                            }

                            if (Props.onlySameType && handler.drone.def != parent.def)
                            {
                                continue;
                            }

                            if (!handler.drone.active)
                            {
                                continue;
                            }

                            handler.drone.Recall();
                        }
                    };
                }

                yield return recallAction;
            }

            if (!parent.active || otherInactiveDrones)
            {
                if (deployAction == null)
                {
                    deployAction = new Command_Action();
                    deployAction.defaultLabel = Props.deployString.Translate(parent.LabelCap);
                    deployAction.icon = cachedDeployTex;
                    deployAction.action = delegate ()
                    {
                        for (int i = Pawn.health.hediffSet.hediffs.Count; i >= 0; i--)
                        {
                            Hediff_DroneHandler handler = Pawn.health.hediffSet.hediffs[i] as Hediff_DroneHandler;

                            if (handler == null)
                            {
                                continue;
                            }

                            if (Props.onlySameType && handler.drone.def != parent.def)
                            {
                                continue;
                            }

                            if (handler.drone.active)
                            {
                                continue;
                            }

                            handler.drone.Deploy();
                        }
                    };
                }

                yield return deployAction;
            }

            yield break;
        }
    }

    public class DroneCompProperties_MassDeployment : DroneCompProperties_ManualDeployment
    {
        // When set to true, instead of deploying/recalling all drones, only drones of the same type would be deployed/recalled
        public bool onlySameType = false;

        public DroneCompProperties_MassDeployment()
        {
            compClass = typeof(DroneComp_MassDeployment);
        }
    }
}
