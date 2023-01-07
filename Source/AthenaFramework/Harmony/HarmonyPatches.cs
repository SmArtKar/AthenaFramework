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
using static HarmonyLib.Code;

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
                if (!__instance.def.HasModExtension<BeamProjectile>() || launcher == null)
                {
                    return;
                }

                MapComponent_AthenaRenderer renderer = __instance.Map.GetComponent<MapComponent_AthenaRenderer>();
                renderer.CreateActiveBeam(launcher, __instance, __instance.def.GetModExtension<BeamProjectile>().beamType, origin - launcher.DrawPos, new Vector3());
            }
        }

        [HarmonyPatch(typeof(Pawn), "SpawnSetup")]
        public static class Pawn_PostSpawnSetup
        {
            static void Postfix(Pawn __instance, Map map, bool respawningAfterLoad)
            {
                if (respawningAfterLoad || __instance.def.GetModExtension<HediffGiverExtension>() == null)
                {
                    return;
                }

                HediffGiverExtension extension = __instance.def.GetModExtension<HediffGiverExtension>();

                foreach (HediffBodypartPair pair in extension.bodypartPairs)
                {
                    if (pair.bodyPartDef == null)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, __instance, null);
                        __instance.health.AddHediff(hediff, null, null, null);
                        continue;
                    }

                    List<BodyPartRecord> partRecords = __instance.RaceProps.body.GetPartsWithDef(pair.bodyPartDef);
                    foreach (BodyPartRecord partRecord in partRecords)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, __instance, partRecord);
                        __instance.health.AddHediff(hediff, partRecord, null, null);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(CompTurretGun), "PostDraw")]
        public static class CompTurretGun_PrePostDraw
        {
            static FieldInfo curRotationField = typeof(CompTurretGun).GetField("curRotation", BindingFlags.NonPublic | BindingFlags.Instance);

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
                matrix4x.SetTRS(__instance.parent.DrawPos + vector, ((float)curRotationField.GetValue(__instance)).ToQuat(), new Vector3(drawSize.x, 0, drawSize.y));
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
                if (!__instance.pawn.RaceProps.Humanlike || __instance.pawn.apparel == null)
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
                float modifier = 1f;
                float offset = 0f;
                List<string> excludedGlobal = new List<string>();

                if (dinfo.Weapon != null && dinfo.Weapon.GetModExtension<DamageAmplifierExtension>() != null)
                {
                    (float, float) result = dinfo.Weapon.GetModExtension<DamageAmplifierExtension>().GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                    modifier *= result.Item1;
                    offset += result.Item2;
                }

                if (dinfo.Instigator == null)
                {
                    dinfo.SetAmount(dinfo.Amount * modifier + offset);
                    return;
                }

                if (dinfo.Instigator.def.GetModExtension<DamageAmplifierExtension>() != null)
                {
                    (float, float) result = dinfo.Instigator.def.GetModExtension<DamageAmplifierExtension>().GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                    modifier *= result.Item1;
                    offset += result.Item2;
                }

                if (dinfo.Instigator is ThingWithComps)
                {
                    foreach (Comp_DamageAmplifier amplifier in (dinfo.Instigator as ThingWithComps).AllComps.OfType<Comp_DamageAmplifier>())
                    {
                        (float, float) result = amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }
                }

                if (dinfo.Instigator is not Pawn)
                {
                    dinfo.SetAmount(dinfo.Amount * modifier + offset);
                    return;
                }

                Pawn pawn = dinfo.Instigator as Pawn;

                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        (float, float) result = hediff.def.GetModExtension<DamageAmplifierExtension>().GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }

                    if (hediff is HediffWithComps)
                    {
                        foreach (HediffComp_DamageAmplifier amplifier in (hediff as HediffWithComps).comps.OfType<HediffComp_DamageAmplifier>())
                        {
                            (float, float) result = amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                            modifier *= result.Item1;
                            offset += result.Item2;
                        }
                    }
                }

                if (pawn.apparel == null)
                {
                    dinfo.SetAmount(dinfo.Amount * modifier + offset);
                    return;
                }

                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    foreach (Comp_DamageAmplifier amplifier in apparel.AllComps.OfType<Comp_DamageAmplifier>())
                    {
                        (float, float) result = amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }

                    if (apparel.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        (float, float) result = apparel.def.GetModExtension<DamageAmplifierExtension>().GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }
                }

                dinfo.SetAmount(dinfo.Amount * modifier + offset);
            }
        }

        [HarmonyPatch(typeof(Projectile), "Impact")]
        public static class Projectile_PreImpact
        {
            static FieldInfo damageModifier = typeof(Projectile).GetField("weaponDamageMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            static void Prefix(Projectile __instance, Thing hitThing, ref bool blockedByShield)
            {
                if (__instance.def.HasModExtension<BeamProjectile>() && __instance.Launcher != null)
                {
                    MapComponent_AthenaRenderer renderer = __instance.Map.GetComponent<MapComponent_AthenaRenderer>();

                    foreach (BeamInfo beamInfo in renderer.activeBeams)
                    {
                        if (beamInfo.beamStart == __instance.Launcher && beamInfo.beamEnd == __instance)
                        {
                            if (hitThing != null)
                            {
                                beamInfo.beam.RenderBeam(beamInfo.beamStart.DrawPos + beamInfo.startOffset, hitThing.DrawPos + beamInfo.endOffset);
                            }
                            else
                            {
                                beamInfo.beam.RenderBeam(beamInfo.beamStart.DrawPos + beamInfo.startOffset, __instance.DrawPos + beamInfo.endOffset);
                            }
                        }
                    }
                }

                if (__instance.def.HasModExtension<ProjectileEffectExtension>())
                {
                    ProjectileEffectExtension effectExtension = __instance.def.GetModExtension<ProjectileEffectExtension>();

                    if (effectExtension.fleck != null)
                    {
                        FleckMaker.Static(__instance.DrawPos, __instance.Map, effectExtension.fleck, 1f);
                    }

                    if (effectExtension.mote != null)
                    {
                        MoteMaker.MakeStaticMote(__instance.DrawPos, __instance.Map, effectExtension.mote, 1f);
                    }
                    if (effectExtension.effecter != null)
                    {
                        effectExtension.effecter.Spawn(__instance.Position, __instance.Map, 1f).Cleanup();
                    }
                }

                if (hitThing == null)
                {
                    return;
                }

                float multiplier = 1f;
                float offset = 0f;
                List<string> excludedGlobal = new List<string>();

                foreach (Comp_DamageAmplifier amplifier in __instance.AllComps.OfType<Comp_DamageAmplifier>())
                {
                    (float, float) result = amplifier.GetDamageModifier(hitThing, ref excludedGlobal, __instance.Launcher);
                    multiplier *= result.Item1;
                    offset += result.Item2;
                }

                if (__instance.def.GetModExtension<DamageAmplifierExtension>() != null)
                {
                    (float, float) result = __instance.def.GetModExtension<DamageAmplifierExtension>().GetDamageModifier(hitThing, ref excludedGlobal, __instance.Launcher);
                    multiplier *= result.Item1;
                    offset += result.Item2;
                }

                float passiveOffset = offset / __instance.DamageAmount;
                damageModifier.SetValue(__instance, (float)damageModifier.GetValue(__instance) * multiplier + passiveOffset);
            }
        }

        [HarmonyPatch(typeof(DamageInfo), nameof(DamageInfo.Amount), MethodType.Getter)]
        public static class DamageInfo_AmountGetter
        {
            static void Postfix(DamageInfo __instance, ref float __result)
            {
                if (__instance.Weapon != null && __instance.Weapon.GetModExtension<DamageAmplifierExtension>() != null)
                {
                    __result *= __instance.Weapon.GetModExtension<DamageAmplifierExtension>().damageMultiplier;
                }

                if (__instance.Instigator == null)
                {
                    return;
                }

                if (__instance.Instigator.def.GetModExtension<DamageAmplifierExtension>() != null)
                {
                    __result *= __instance.Instigator.def.GetModExtension<DamageAmplifierExtension>().damageMultiplier;
                }

                if (__instance.Instigator is ThingWithComps)
                {
                    foreach (Comp_DamageAmplifier amplifier in (__instance.Instigator as ThingWithComps).AllComps.OfType<Comp_DamageAmplifier>())
                    {
                        __result *= amplifier.DamageMultiplier;
                    }
                }

                if (__instance.Instigator is not Pawn)
                {
                    return;
                }

                Pawn pawn = __instance.Instigator as Pawn;

                foreach (Hediff hediff in pawn.health.hediffSet.hediffs)
                {
                    if (hediff.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        __result *= hediff.def.GetModExtension<DamageAmplifierExtension>().damageMultiplier;
                    }

                    if (hediff is HediffWithComps)
                    {
                        foreach (HediffComp_DamageAmplifier amplifier in (hediff as HediffWithComps).comps.OfType<HediffComp_DamageAmplifier>())
                        {
                            __result *= amplifier.DamageMultiplier;
                        }
                    }
                }

                if (pawn.apparel == null)
                {
                    return;
                }

                foreach (Apparel apparel in pawn.apparel.WornApparel)
                {
                    foreach (Comp_DamageAmplifier amplifier in apparel.AllComps.OfType<Comp_DamageAmplifier>())
                    {
                        __result *= amplifier.DamageMultiplier;
                    }

                    if (apparel.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        __result *= apparel.def.GetModExtension<DamageAmplifierExtension>().damageMultiplier;
                    }
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
        public static class StunHandler_PreDamageApplied
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
