using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.UI.Image;

namespace AthenaFramework
{
    [HarmonyPatch(typeof(Projectile), "Launch", new System.Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
    public static class Projectile_PostLaunch
    {
        public static void Postfix(Projectile __instance, Thing launcher, ref Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            for (int i = __instance.AllComps.Count - 1; i >= 0; i--)
            {
                ProjectileComp comp = __instance.AllComps[i] as ProjectileComp;

                if (comp != null)
                {
                    comp.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), "CanHit")]
    public static class Projectile_PostCanHit
    {
        static void Postfix(Projectile __instance, Thing thing, ref bool __result)
        {
            for (int i = __instance.AllComps.Count - 1; i >= 0; i--)
            {
                ProjectileComp comp = __instance.AllComps[i] as ProjectileComp;

                if (comp != null)
                {
                    comp.CanHit(thing, ref __result);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), "Impact")]
    public static class Projectile_PreImpact
    {
        static void Prefix(Projectile __instance, Thing hitThing, ref bool blockedByShield)
        {
            float multiplier = 1f;
            float offset = 0f;
            List<string> excludedGlobal = new List<string>();

            if (__instance.def.GetModExtension<DamageModifierExtension>() != null)
            {
                multiplier *= __instance.def.GetModExtension<DamageModifierExtension>().OutgoingDamageMultiplier;
            }

            for (int i = __instance.AllComps.Count - 1; i >= 0; i--)
            {
                ProjectileComp comp = __instance.AllComps[i] as ProjectileComp;

                if (comp != null)
                {
                    comp.Impact(hitThing, ref blockedByShield);
                }

                Comp_DamageModifier dmgMod = __instance.AllComps[i] as Comp_DamageModifier;

                if (dmgMod != null)
                {
                    (float, float) result = dmgMod.GetOutcomingDamageModifier(hitThing, ref excludedGlobal, __instance.Launcher, null, true);
                    multiplier *= result.Item1;
                    offset += result.Item2;
                }
            }

            __instance.weaponDamageMultiplier = __instance.weaponDamageMultiplier * multiplier + offset / __instance.DamageAmount;
        }
    }
}
