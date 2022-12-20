using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using System.Reflection;

namespace AthenaFramework
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            Harmony harmony = new Harmony(id: "rimworld.smartkar.athenaframework.main");
            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(CompTurretGun), "TurretMat", MethodType.Getter)]
        public static class CompTurretGun_TurretMat_Fixer
        {
            static void Prefix(CompTurretGun __instance)
            {
                if (__instance.turretMat != null)
                {
                    return;
                }

                if (!__instance.gun.def.HasModExtension<TurretGraphicOverride>())
                {
                    return;
                }

                CompProperties_TurretGun props = __instance.props as CompProperties_TurretGun;

                __instance.turretMat = props.turretDef.graphicData.Graphic.MatSingle;
            }
        }

        [HarmonyPatch(typeof(CompTurretGun), "CanShoot", MethodType.Getter)]
        public static class CompTurretGun_CanShootGetter
        {
            static void Postfix(CompTurretGun __instance, ref bool __result)
            {
                if (!__instance.gun.def.HasModExtension<TurretRoofBlocked>())
                {
                    return;
                }

                Pawn pawn = __instance.parent as Pawn;

                if(pawn == null)
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

        [HarmonyPatch(typeof(Projectile), "Launch", new Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
        public static class Projectile_PostLaunch
        {
            static void Postfix(Projectile __instance, Thing launcher, ref Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags)
            {
                if (!__instance.def.HasModExtension<BeamProjectile>())
                {
                    return;
                }

                MapComponent_AthenaRenderer renderer = __instance.Map.GetComponent<MapComponent_AthenaRenderer>();
                renderer.CreateActiveBeam(launcher, __instance, __instance.def.GetModExtension<BeamProjectile>().beamType, origin - launcher.DrawPos, new Vector3());
            }
        }

        [HarmonyPatch(typeof(CompTurretGun), "PostDraw")]
        public static class CompTurretGun_PrePostDraw
        {
            static bool Prefix(CompTurretGun __instance)
            {
                if (!__instance.gun.def.HasModExtension<TurretGraphicOverride>())
                {
                    return true;
                }

                CompProperties_TurretGun props = __instance.props as CompProperties_TurretGun;

                if (__instance.turretMat == null)
                {
                    __instance.turretMat = props.turretDef.graphicData.Graphic.MatSingle;
                }
                TurretGraphicOverride graphicOverride = __instance.gun.def.GetModExtension<TurretGraphicOverride>();

                Rot4 rotation = __instance.parent.Rotation;
                Vector3 vector = new Vector3(0f, 0.04054054f, 0f);
                if (graphicOverride.offsets != null)
                {
                    if (graphicOverride.offsets.Count == 4)
                    {
                        vector += graphicOverride.offsets[rotation.AsInt];
                    }
                    else
                    {
                        vector += graphicOverride.offsets[0];
                    }
                }
                Matrix4x4 matrix4x = default(Matrix4x4);
                Vector2 drawSize = props.turretDef.graphicData.drawSize;
                matrix4x.SetTRS(__instance.parent.DrawPos + vector, ((float)typeof(CompTurretGun).GetField("curRotation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance)).ToQuat(), new Vector3(drawSize.x, 0, drawSize.y));
                Graphics.DrawMesh(MeshPool.plane10, matrix4x, __instance.turretMat, 0);
                return false;
            }
        }

        [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
        public static class ApparelGraphicRecordGetter_PostTryGetGraphicApparel
        {
            static bool Prefix(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec, ref bool __result)
            {
                Comp_CustomApparelBody customBody = apparel.GetComp<Comp_CustomApparelBody>();
                if (customBody == null || !(customBody.props as CompProperties_CustomApparelBody).preventBodytype)
                {
                    return true;
                }

                if (apparel.WornGraphicPath.NullOrEmpty())
                {
                    return true;
                }

                Shader shader = ShaderDatabase.Cutout;
                if (apparel.def.apparel.useWornGraphicMask)
                {
                    shader = ShaderDatabase.CutoutComplex;
                }

                Graphic graphic = GraphicDatabase.Get<Graphic_Multi>(apparel.WornGraphicPath, shader, apparel.def.graphicData.drawSize, apparel.DrawColor);
                rec = new ApparelGraphicRecord(graphic, apparel);

                __result = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
        public static class PawnGraphicSet_PostResolveAllGraphics_Postfix
        {
            static void Postfix(PawnGraphicSet __instance)
            {
                if (!__instance.pawn.RaceProps.Humanlike)
                {
                    return;
                }

                bool bodyGraphicsSet = false;
                bool headGraphicsSet = false;

                foreach (Comp_CustomApparelBody customBody in __instance.pawn.apparel.WornApparel.SelectMany((Apparel x) => x.AllComps).OfType<Comp_CustomApparelBody>())
                {
                    Graphic customBodyGraphic = customBody.getBodyGraphic;
                    if (customBodyGraphic != null)
                    {
                        __instance.nakedGraphic = customBodyGraphic;
                        bodyGraphicsSet = true;
                    }

                    Graphic customHeadGraphic = customBody.getHeadGraphic;
                    if (customHeadGraphic != null)
                    {
                        __instance.headGraphic = customHeadGraphic;
                        headGraphicsSet = true;
                    }

                    BodyTypeDef customBodytype = customBody.getBodytype;
                    if (customBodytype != null && !bodyGraphicsSet)
                    {

                    }
                }

                if (bodyGraphicsSet || headGraphicsSet)
                {
                    __instance.CalculateHairMats();
                    __instance.ResolveApparelGraphics();
                    __instance.ResolveGeneGraphics();
                }
            }
        }

        // Damage patches

        [HarmonyPatch(typeof(DamageInfo), nameof(DamageInfo.Amount), MethodType.Getter)]
        public static class DamageInfo_AmountGetter
        {
            static void Postfix(DamageInfo __instance, ref float __result)
            {
                if (__instance.Instigator == null || !(__instance.Instigator is Pawn))
                {
                    return;
                }

                Pawn pawn = __instance.Instigator as Pawn;
                foreach (HediffComp_DamageAmplifier amplifier in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_DamageAmplifier>())
                {
                    __result *= amplifier.DamageMultiplier;
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.PreApplyDamage))]
        public static class Pawn_PostPreApplyDamage
        {
            static void Postfix(Pawn __instance, ref DamageInfo dinfo, ref bool absorbed)
            {
                foreach (HediffComp_Shield shield in __instance.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_Shield>())
                {
                    shield.BlockDamage(ref dinfo, ref absorbed);

                    if (absorbed)
                    {
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied")]
        public static class ThingWithComps_PreApplyDamage
        {
            static bool Prefix(StunHandler __instance, ref DamageInfo dinfo)
            {
                if (!(__instance.parent is Pawn))
                {
                    return true;
                }

                Pawn pawn = __instance.parent as Pawn;

                foreach (HediffComp_Shield shield in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_Shield>())
                {
                    if (shield.BlockStun(ref dinfo))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        // Rendering patches

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt))]
        public static class Pawn_PostDrawAt
        {
            static void Postfix(Pawn __instance, Vector3 drawLoc)
            {
                foreach (HediffComp_Renderable renderable in __instance.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_Renderable>())
                {
                    renderable.DrawAt(drawLoc);
                }
            }
        }
    }
}
