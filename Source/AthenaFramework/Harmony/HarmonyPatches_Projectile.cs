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
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Launch), new System.Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
    public static class Projectile_PostLaunch
    {
        public static void Postfix(Projectile __instance, Thing launcher, ref Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags, bool preventFriendlyFire, Thing equipment, ThingDef targetCoverDef)
        {
            if (!AthenaCache.projectileCache.TryGetValue(__instance.thingIDNumber, out List<IProjectile> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IProjectile projectile = mods[i];
                projectile.Launch(launcher, origin, usedTarget, intendedTarget, hitFlags, preventFriendlyFire, equipment, targetCoverDef);
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.CanHit))]
    public static class Projectile_PostCanHit
    {
        static void Postfix(Projectile __instance, Thing thing, ref bool __result)
        {
            if (!AthenaCache.projectileCache.TryGetValue(__instance.thingIDNumber, out List<IProjectile> mods))
            {
                return;
            }

            for (int i = mods.Count - 1; i >= 0; i--)
            {
                IProjectile projectile = mods[i];
                projectile.CanHit(thing, ref __result);
            }
        }
    }

    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Impact))]
    public static class Projectile_PreImpact
    {
        static void Prefix(Projectile __instance, Thing hitThing, ref bool blockedByShield)
        {
            if (__instance.def == null || __instance.DamageAmount == 0)
            {
                return;
            }

            float multiplier = 1f;
            float offset = 0f;
            List<string> excludedGlobal = new List<string>();

            if (__instance.def.GetModExtension<DamageModifierExtension>() != null)
            {
                multiplier *= __instance.def.GetModExtension<DamageModifierExtension>().OutgoingDamageMultiplier;
            }

            if (AthenaCache.projectileCache.TryGetValue(__instance.thingIDNumber, out List<IProjectile> mods))
            {
                for (int i = mods.Count - 1; i >= 0; i--)
                {
                    IProjectile projectile = mods[i];
                    projectile.Impact(hitThing, ref blockedByShield);
                }
            }

            if (AthenaCache.damageCache.TryGetValue(__instance.thingIDNumber, out List<IDamageModifier> mods2))
            {
                for (int i = mods2.Count - 1; i >= 0; i--)
                {
                    IDamageModifier modifierComp = mods2[i];

                    (float, float) result = modifierComp.GetOutcomingDamageModifier(hitThing, ref excludedGlobal, __instance.Launcher, null, true);
                    multiplier *= result.Item1;
                    offset += result.Item2;
                }
            }

            __instance.weaponDamageMultiplier = __instance.weaponDamageMultiplier * multiplier + offset / __instance.DamageAmount;
        }
    }
}
