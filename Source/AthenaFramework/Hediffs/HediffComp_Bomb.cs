using Mono.Unix.Native;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace AthenaFramework
{
    public class HediffComp_Bomb : HediffComp
    {
        private HediffCompProperties_Bomb Props => props as HediffCompProperties_Bomb;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();

            if (Pawn == null)
            {
                return;
            }

            if (!Props.explodeOnRemoval)
            {
                return;
            }

            if (Props.gasType == GasType.ToxGas && !ModLister.BiotechInstalled)
            {
                return;
            }

            if (Pawn == null || !Pawn.Spawned)
            {
                return;
            }

            GenExplosion.DoExplosion(Pawn.Position, Pawn.Map, Props.radius, Props.damageDef, Pawn, postExplosionGasType: (Props.gasExplosion ? new GasType?(Props.gasType) : null));
        }

        public override void Notify_PawnKilled()
        {
            base.Notify_PawnKilled();
            if (!Props.explodeOnDeath)
            {
                return;
            }

            GenExplosion.DoExplosion(Pawn.Position, Pawn.Map, Props.radius, Props.damageDef, Pawn, postExplosionGasType: (Props.gasExplosion ? new GasType?(Props.gasType) : null));
            Pawn.health.RemoveHediff(parent);
        }

        public HediffComp_Bomb() { }
    }

    public class HediffCompProperties_Bomb : HediffCompProperties
    {
        public HediffCompProperties_Bomb()
        {
            this.compClass = typeof(HediffComp_Bomb);
        }

        // Radius of the explosion
        public float radius;
        // Whenever the bomb will explode upon removal
        public bool explodeOnRemoval = true;
        // Whenever the bomb will explode upon owner's death
        public bool explodeOnDeath = true;
        // Damage type of the bomb
        public DamageDef damageDef;
        // When set to true,
        public bool gasExplosion = false;
        // You can set this to whatever type of gas you want. ToxGas will require user to have biotech installed.
        public GasType gasType;
    }
}
