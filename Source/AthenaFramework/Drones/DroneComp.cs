﻿using RimWorld;
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

        public virtual void OnDestroyed()
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

        public virtual void OnSetup()
        {
        }

        public virtual void TargetUpdate()
        {
            return;
        }

        public virtual IntVec3? PositionOverride()
        {
            return null;
        }

        public virtual Vector3 DrawPosOffset()
        {
            return Vector3.zero;
        }

        public virtual bool DrawPosOverride(ref Vector3 pos)
        {
            return false;
        }

        public virtual bool DisableHoveringAnimation()
        {
            return false;
        }

        public virtual void PrePawnApplyDamage(ref DamageInfo dinfo, ref float hitChance, ref bool absorbed) { }

        public virtual void TryGetStat(StatDef stat, ref float value) { }

        public virtual bool RecacheTarget(out LocalTargetInfo newTarget, out float newPriority, float rangeOverride = -1f, List<LocalTargetInfo> blacklist = null)
        {
            newTarget = null;
            newPriority = 0f;
            return false;
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
