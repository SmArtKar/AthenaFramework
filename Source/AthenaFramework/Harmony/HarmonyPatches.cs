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
                if (customBody == null || !customBody.getPreventBodytype)
                {
                    if (apparel.Wearer != null)
                    {
                        foreach (Apparel wornApparel in apparel.Wearer.apparel.WornApparel)
                        {
                            Comp_CustomApparelBody wornCustomBody = wornApparel.GetComp<Comp_CustomApparelBody>();
                            if (wornCustomBody != null && wornCustomBody.getBodytype != null)
                            {
                                bodyType = wornCustomBody.getBodytype;
                                return true;
                            }
                        }
                    }

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

                bool graphicsSet = false;
                BodyTypeDef customUserBody = null;

                foreach (Comp_CustomApparelBody customBody in __instance.pawn.apparel.WornApparel.SelectMany((Apparel x) => x.AllComps).OfType<Comp_CustomApparelBody>())
                {
                    Graphic customBodyGraphic = customBody.getBodyGraphic;
                    if (customBodyGraphic != null)
                    {
                        __instance.nakedGraphic = customBodyGraphic;
                        graphicsSet = true;
                    }

                    Graphic customHeadGraphic = customBody.getHeadGraphic;
                    if (customHeadGraphic != null)
                    {
                        __instance.headGraphic = customHeadGraphic;
                        graphicsSet = true;
                    }

                    BodyTypeDef customBodytype = customBody.getBodytype;
                    if (customBodytype != null)
                    {
                        customUserBody = customBodytype;
                    }
                }

                if (graphicsSet)
                {
                    __instance.CalculateHairMats();
                    __instance.ResolveApparelGraphics();
                    __instance.ResolveGeneGraphics();
                }
                else if (customUserBody != null && __instance.pawn != null)
                {
                    LongEventHandler.ExecuteWhenFinished(delegate
                    {
                        Color color = (__instance.pawn.story.SkinColorOverriden ? (PawnGraphicSet.RottingColorDefault * __instance.pawn.story.SkinColor) : PawnGraphicSet.RottingColorDefault);
                        __instance.nakedGraphic = GraphicDatabase.Get<Graphic_Multi>(customUserBody.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, __instance.pawn.story.SkinColor);
                        __instance.rottingGraphic = GraphicDatabase.Get<Graphic_Multi>(customUserBody.bodyNakedGraphicPath, ShaderUtility.GetSkinShader(__instance.pawn.story.SkinColorOverriden), Vector2.one, color);
                        __instance.dessicatedGraphic = GraphicDatabase.Get<Graphic_Multi>(customUserBody.bodyDessicatedGraphicPath, ShaderDatabase.Cutout);

                        __instance.CalculateHairMats();
                        __instance.ResolveApparelGraphics();
                        __instance.ResolveGeneGraphics();
                    });
                }
            }
        }

        // Damage patches

        [HarmonyPatch(typeof(Thing), nameof(Thing.PreApplyDamage))]
        public static class Thing_PrePreApplyDamage
        {
            static void Prefix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
            {
                if (dinfo.Instigator == null)
                {
                    return;
                }

                float modifier = 1f;
                List<string> excludedGlobal = new List<string>();

                if (dinfo.Instigator is not Pawn)
                {
                    if (dinfo.Instigator is not ThingWithComps)
                    {
                        return;
                    }

                    foreach (Comp_DamageAmplifier amplifier in (dinfo.Instigator as ThingWithComps).AllComps.OfType<Comp_DamageAmplifier>())
                    {
                        modifier *= amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                    }

                    return;
                }

                Pawn pawn = dinfo.Instigator as Pawn;
                foreach (HediffComp_DamageAmplifier amplifier in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_DamageAmplifier>())
                {
                    modifier *= amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                }

                foreach (Comp_DamageAmplifier amplifier in pawn.apparel.WornApparel.SelectMany((Apparel x) => x.AllComps).OfType<Comp_DamageAmplifier>().Concat(pawn.AllComps.OfType<Comp_DamageAmplifier>()))
                {
                    modifier *= amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                }
            }
        }

        [HarmonyPatch(typeof(Bullet), "Impact")]
        public static class Bullet_PreImpact
        {
            static void Prefix(Bullet __instance, Thing hitThing, ref bool blockedByShield)
            {
                if (hitThing == null)
                {
                    return;
                }

                float multiplier = 1f;
                List<string> excludedGlobal = new List<string>();

                foreach (Comp_DamageAmplifier amplifier in __instance.AllComps.OfType<Comp_DamageAmplifier>())
                {
                    multiplier *= amplifier.GetDamageModifier(hitThing, ref excludedGlobal, __instance.Launcher);
                }

                FieldInfo damageModifier = typeof(Bullet).GetField("weaponDamageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
                damageModifier.SetValue(__instance, (float)damageModifier.GetValue(__instance) * multiplier);
            }
        }

        [HarmonyPatch(typeof(DamageInfo), nameof(DamageInfo.Amount), MethodType.Getter)]
        public static class DamageInfo_AmountGetter
        {
            static void Postfix(DamageInfo __instance, ref float __result)
            {
                if (__instance.Instigator == null)
                {
                    return;
                }

                if (__instance.Instigator is not Pawn)
                {
                    if (__instance.Instigator is not ThingWithComps)
                    {
                        return;
                    }

                    foreach (Comp_DamageAmplifier amplifier in (__instance.Instigator as ThingWithComps).AllComps.OfType<Comp_DamageAmplifier>())
                    {
                        __result *= amplifier.DamageMultiplier;
                    }

                    return;
                }

                Pawn pawn = __instance.Instigator as Pawn;
                foreach (HediffComp_DamageAmplifier amplifier in pawn.health.hediffSet.hediffs.OfType<HediffWithComps>().SelectMany((HediffWithComps x) => x.comps).OfType<HediffComp_DamageAmplifier>())
                {
                    __result *= amplifier.DamageMultiplier;
                }

                foreach (Comp_DamageAmplifier amplifier in pawn.apparel.WornApparel.SelectMany((Apparel x) => x.AllComps).OfType<Comp_DamageAmplifier>().Concat(pawn.AllComps.OfType<Comp_DamageAmplifier>()))
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
