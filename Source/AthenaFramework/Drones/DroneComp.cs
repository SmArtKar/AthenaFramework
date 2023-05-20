using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    public class DroneComp
    {
        public DroneCompProperties props;
        public Drone parent;

        protected Pawn Pawn => parent.pawn;

        public virtual void Initialize(DroneCompProperties props)
        {
            this.props = props;
        }

        public virtual List<DroneGraphicPackage> GetAdditionalGraphics()
        {
            return new List<DroneGraphicPackage>();
        }

        public virtual void Tick()
        {
        }

        // Only called when the drone is active
        public virtual void ActiveTick()
        {
        }

        public virtual IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield break;
        }

        public virtual void CompExposeData()
        {
        }

        public virtual void OnDeployed()
        {
        }
        public virtual void OnRecalled()
        {
        }

        public virtual void Destroyed()
        {
        }

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
        }

        public virtual void PreApplyDamage(DamageInfo dinfo)
        {
        }

        public virtual void PostApplyDamage(DamageInfo dinfo)
        {
        }

        public virtual (LocalTargetInfo, float) GetNewTarget()
        {
            return (null, 0f);
        }
    }

    public class DroneCompProperties
    {
        [TranslationHandle]
        public Type compClass;

        public virtual void PostLoad()
        {
        }

        public virtual void ResolveReferences(DroneDef parent)
        {
        }

        public virtual IEnumerable<string> ConfigErrors(DroneDef parentDef)
        {
            if (compClass == null)
            {
                yield return "compClass is null";
            }
        }
    }
}
