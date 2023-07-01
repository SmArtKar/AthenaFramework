using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class Verb_ShootMinigun : Verb_Shoot
    {
        public float speedModifier = 0f;
        public IntVec3 lastPosition;
        public bool startedFiring = false;
        public MinigunExtension cachedExtension;

        public MinigunExtension Extension
        {
            get
            {
                if (cachedExtension == null)
                {
                    if (HediffSource != null)
                    {
                        cachedExtension = HediffSource.def.GetModExtension<MinigunExtension>();
                    }
                    else
                    {
                        cachedExtension = EquipmentSource.def.GetModExtension<MinigunExtension>();
                    }
                }

                return cachedExtension;
            }
        }

        public override int ShotsPerBurst => Extension.unlimitedBurst ? 2 : base.ShotsPerBurst;

        public override void BurstingTick()
        {
            base.BurstingTick();

            if (Extension.standingOnly)
            {
                if (!startedFiring)
                {
                    lastPosition = Caster.Position;
                    startedFiring = true;
                }

                if (lastPosition != Caster.Position)
                {
                    Reset();
                    return;
                }
            }

            if (CasterIsPawn)
            {
                if (!CasterPawn.Drafted || CasterPawn.TargetCurrentlyAimingAt == null || CasterPawn.stances.stunner.Stunned || CasterPawn.Downed)
                {
                    Reset();
                    return;
                }
            }

            if (burstShotsLeft == 1 && Extension.unlimitedBurst)
            {
                burstShotsLeft = 2;
            }

            ticksToNextBurstShot = Math.Max(0, ticksToNextBurstShot - (int)speedModifier);
            speedModifier += Extension.speedPerTick;
        }

        public override void Reset()
        {
            base.Reset();
            speedModifier = 0f;
            startedFiring = false;
        }
    }

    public class MinigunExtension : DefModExtension
    {
        // Use NonInterruptingSelfCast or else the pawn won't be able to stop bursting

        // Maximum ramp up speed. Deducted from ticksBetweenBurstShots every tick. Should not be lower than ticksBetweenBurstShots
        public float maxSpeed = 5f;
        // Burst speed gain every tick.
        public float speedPerTick = 0.02f;
        // If the gun can only be used when standing still
        public bool standingOnly = true;
        // When set to true, pawn will ignore shotsPerBurst and fire until something stops them.
        public bool unlimitedBurst = true;
    }
}
