using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using static HarmonyLib.Code;
using UnityEngine;
using AthenaFramework.Gizmos;

namespace AthenaFramework
{
    public class CompHediff_Shield : CompHediff_Renderable
    {
        public float energy = 75f;

        private HediffCompProperties_Shield Props => props as HediffCompProperties_Shield;
        private Matrix4x4 matrix;
        private Gizmo_HediffShieldStatus gizmo;

        public float EnergyPercent
        {
            get
            {
                return energy / Props.maxEnergy;
            }
        }

        public override void DrawAt(Vector3 drawPos)
        {
            if (Props.graphicData == null)
            {
                return;
            }

            drawPos.y = AltitudeLayer.MoteOverhead.AltitudeFor();
            float scale = Props.minDrawSize + (Props.maxDrawSize - Props.minDrawSize) * EnergyPercent;
            matrix.SetTRS(drawPos, Quaternion.AngleAxis(Rand.Range(0, 360), Vector3.up), new Vector3(scale, 1f, scale));
            Graphics.DrawMesh(MeshPool.plane10, matrix, Props.graphicData.Graphic.MatSingle, 0);
        }

        public override IEnumerable<Gizmo> CompGetGizmos()
        {
            if (Props.displayGizmo)
            {
                if (parent.pawn.Faction.IsPlayer && Find.Selector.SingleSelectedThing == parent.pawn)
                {
                    if (gizmo == null)
                    {
                        gizmo = new Gizmo_HediffShieldStatus();
                        gizmo.shieldHediff = this;
                    }
                    yield return gizmo;
                }
            }

            yield break;
        }

        public override void CompExposeData()
        {
            base.CompExposeData();
            Scribe_Values.Look(ref energy, "energy");
        }
    }

    public class HediffCompProperties_Shield : HediffCompProperties_Renderable
    {
        public HediffCompProperties_Shield()
        {
            this.compClass = typeof(CompHediff_Shield);
        }

        public float maxEnergy;
        public bool displayGizmo = true;
        public string gizmoTitle = "";
        public string gizmoTip = "";

        public float minDrawSize;
        public float maxDrawSize;
    }
}
