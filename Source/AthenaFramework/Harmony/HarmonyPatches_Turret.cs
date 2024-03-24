using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace AthenaFramework
{
    [HarmonyPatch(typeof(CompTurretGun), nameof(CompTurretGun.CanShoot), MethodType.Getter)]
    public static class CompTurretGun_CanShootGetter
    {
        public static void Postfix(CompTurretGun __instance, ref bool __result)
        {
            if (!__instance.gun.def.HasModExtension<TurretRoofBlocked>())
            {
                return;
            }

            Pawn pawn = __instance.parent as Pawn;

            if (pawn == null)
            {
                return;
            }

            RoofDef roof = pawn.Position.GetRoof(pawn.MapHeld);

            if (roof != null)
            {
                __result = false;
            }
        }
    }
}
