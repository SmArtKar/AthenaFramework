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
    public class HediffComp_GasBomb : HediffComp
    {
        private HediffCompProperties_GasBomb Props => props as HediffCompProperties_GasBomb;

        public override void CompPostPostRemoved()
        {
            base.CompPostPostRemoved();


            if (Props.gasType == GasType.ToxGas && !ModLister.BiotechInstalled)
            {
                return;
            }

            if (parent.pawn == null || !parent.pawn.Spawned)
            {
                return;
            }

            GenExplosion.DoExplosion(parent.pawn.Position, parent.pawn.Map, Props.radius, Props.damageDef, parent.pawn, postExplosionGasType: new GasType?(Props.gasType));
        }
    }

    public class HediffCompProperties_GasBomb : HediffCompProperties
    {
        public HediffCompProperties_GasBomb()
        {
            this.compClass = typeof(HediffComp_GasBomb);
        }

        public float radius;
        public DamageDef damageDef = DamageDefOf.ToxGas;
        public GasType gasType = GasType.ToxGas;
    }
}
