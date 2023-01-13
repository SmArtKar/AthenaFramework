using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static HarmonyLib.Code;
using static UnityEngine.UI.Image;
using RimWorld;
using Verse;
using UnityEngine;

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

        [HarmonyPatch(typeof(Projectile), "Launch", new System.Type[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
        public static class Projectile_PostLaunch
        {
            static void Postfix(Projectile __instance, Thing launcher, ref Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget, ProjectileHitFlags hitFlags)
            {
                Comp_BeamProjectile comp = __instance.TryGetComp<Comp_BeamProjectile>();
                if (comp != null && launcher != null)
                {
                    comp.beam = Beam.CreateActiveBeam(launcher, __instance, comp.Props.beamDef, origin - launcher.DrawPos);
                }
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

                for (int i = extension.bodypartPairs.Count- 1; i >= 0; i--)
                {
                    HediffBodypartPair pair = extension.bodypartPairs[i];

                    if (pair.bodyPartDef == null)
                    {
                        Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, __instance, null);
                        __instance.health.AddHediff(hediff, null, null, null);
                        continue;
                    }

                    List<BodyPartRecord> partRecords = __instance.RaceProps.body.GetPartsWithDef(pair.bodyPartDef);
                    for (int j = partRecords.Count-1; j >= 0; j--)
                    {
                        BodyPartRecord partRecord = partRecords[j];
                        Hediff hediff = HediffMaker.MakeHediff(pair.hediffDef, __instance, partRecord);
                        __instance.health.AddHediff(hediff, partRecord, null, null);
                    }
                }
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
                matrix4x.SetTRS(__instance.parent.DrawPos + vector, __instance.curRotation.ToQuat(), new Vector3(drawSize.x, 0, drawSize.y));
                Graphics.DrawMesh(MeshPool.plane10, matrix4x, __instance.turretMat, 0);
                return false;
            }
        }

        [HarmonyPatch(typeof(ApparelGraphicRecordGetter), "TryGetGraphicApparel")]
        public static class ApparelGraphicRecordGetter_PostTryGetGraphicApparel
        {
            static bool Prefix(ref Apparel apparel, ref BodyTypeDef bodyType, ref ApparelGraphicRecord rec, ref bool __result)
            {
                for (int i = apparel.AllComps.Count - 1; i >= 0; i--)
                {
                    Comp_CustomApparelBody customBody = apparel.AllComps[i] as Comp_CustomApparelBody;

                    if (customBody == null)
                    {
                        continue;
                    }

                    if (customBody.PreventBodytype(bodyType, rec))
                    {
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

                    BodyTypeDef newBodyType = customBody.CustomBodytype(apparel, bodyType, rec);

                    if (newBodyType != null)
                    {
                        bodyType = newBodyType;
                        return true;
                    }
                }

                if (apparel.Wearer == null || apparel.Wearer.apparel == null) //Somehow, this happened, be that another mod's intervention or something else.
                {
                    return true;
                }

                List<Apparel> wornApparel = apparel.Wearer.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel otherApparel = wornApparel[i];

                    for (int j = otherApparel.AllComps.Count - 1; j >= 0; j--)
                    {
                        Comp_CustomApparelBody customBody = otherApparel.AllComps[j] as Comp_CustomApparelBody;

                        if (customBody == null)
                        {
                            continue;
                        }

                        BodyTypeDef newBodyType = customBody.CustomBodytype(apparel, bodyType, rec);

                        if (newBodyType != null)
                        {
                            bodyType = newBodyType;
                            return true;
                        }
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
        public static class PawnGraphicSet_PostResolveAllGraphics
        {
            static void Postfix(PawnGraphicSet __instance)
            {
                if (!__instance.pawn.RaceProps.Humanlike || __instance.pawn.apparel == null)
                {
                    return;
                }

                bool graphicsSet = false;
                BodyTypeDef customUserBody = null;

                List<Apparel> wornApparel = __instance.pawn.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];

                    for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                    {
                        Comp_CustomApparelBody customBody = apparel.AllComps[j] as Comp_CustomApparelBody;
                        
                        if (customBody == null)
                        {
                            continue;
                        }

                        Graphic customBodyGraphic = customBody.GetBodyGraphic;

                        if (customBodyGraphic != null)
                        {
                            __instance.nakedGraphic = customBodyGraphic;
                            graphicsSet = true;
                        }

                        Graphic customHeadGraphic = customBody.GetHeadGraphic;

                        if (customHeadGraphic != null)
                        {
                            __instance.headGraphic = customHeadGraphic;
                            graphicsSet = true;
                        }

                        BodyTypeDef customBodytype = customBody.CustomBodytype(apparel, __instance.pawn.story.bodyType);

                        if (customBodytype != null)
                        {
                            customUserBody = customBodytype;
                        }
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

                ThingWithComps compThing = dinfo.Instigator as ThingWithComps;

                if (compThing == null)
                {
                    dinfo.SetAmount(dinfo.Amount * modifier + offset);
                    return;
                }

                for (int i = compThing.AllComps.Count - 1; i >= 0; i--)
                {
                    Comp_DamageAmplifier amplifier = compThing.AllComps[i] as Comp_DamageAmplifier;

                    if (amplifier == null)
                    {
                        continue;
                    }

                    (float, float) result = amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                    modifier *= result.Item1;
                    offset += result.Item2;
                }

                Pawn pawn = dinfo.Instigator as Pawn;

                if (pawn == null)
                {
                    dinfo.SetAmount(dinfo.Amount * modifier + offset);
                    return;
                }

                for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 1; i--)
                {
                    Hediff hediff = pawn.health.hediffSet.hediffs[i];

                    if (hediff.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        (float, float) result = hediff.def.GetModExtension<DamageAmplifierExtension>().GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }

                    HediffWithComps compHediff = hediff as HediffWithComps;

                    if (compHediff == null)
                    {
                        continue;

                    }

                    for (int j = compHediff.comps.Count- 1; j >= 0; j--)
                    {
                        HediffComp_DamageAmplifier amplifier = compHediff.comps[j] as HediffComp_DamageAmplifier;

                        if (amplifier == null)
                        {
                            continue;
                        }

                        (float, float) result = amplifier.GetDamageModifier(__instance, ref excludedGlobal, dinfo.Instigator);
                        modifier *= result.Item1;
                        offset += result.Item2;
                    }
                }

                if (pawn.apparel == null)
                {
                    dinfo.SetAmount(dinfo.Amount * modifier + offset);
                    return;
                }

                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];

                    for (int j = apparel.AllComps.Count- 1; j >= 0; j--)
                    {
                        Comp_DamageAmplifier amplifier = apparel.AllComps[j] as Comp_DamageAmplifier;

                        if (amplifier == null)
                        {
                            continue;
                        }

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

            static void Postfix(Thing __instance, ref DamageInfo dinfo, ref bool absorbed)
            {
                Pawn pawn = __instance as Pawn;
                if (pawn == null || absorbed)
                {
                    return;
                }

                for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    HediffWithComps hediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                    if (hediff == null)
                    {
                        continue;
                    }

                    for (int j = hediff.comps.Count - 1; j >= 0; j--)
                    {
                        HediffComp_Shield shield = hediff.comps[j] as HediffComp_Shield;

                        if (shield != null)
                        {
                            shield.BlockDamage(ref dinfo, ref absorbed);

                            if (absorbed)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PawnRenderer), "DrawEquipmentAiming")]
        public static class PawnRenderer_DrawEquipmentAiming_Offset
        {
            public static void Prefix(PawnRenderer __instance, Thing eq, ref Vector3 drawLoc, ref float aimAngle)
            {
                ThingWithComps thing = eq as ThingWithComps;

                if (thing == null)
                {
                    return;
                }

                AimAngleOffsetExtension ext = thing.def.GetModExtension<AimAngleOffsetExtension>();

                if (ext != null)
                {
                    aimAngle += ext.angleOffset;
                }
            }
        }

        [HarmonyPatch(typeof(Projectile), "Impact")]
        public static class Projectile_PreImpact
        {
            static void Prefix(Projectile __instance, Thing hitThing, ref bool blockedByShield)
            {
                Comp_BeamProjectile comp = __instance.TryGetComp<Comp_BeamProjectile>();
                if (comp != null && __instance.Launcher != null)
                {
                    comp.beam.AdjustBeam(__instance.Launcher.DrawPos + comp.beam.startOffset, __instance.DrawPos);
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
                List<string> excludedGlobal =  new List<string>();

                for (int i = __instance.AllComps.Count - 1; i >= 0; i--)
                {
                    Comp_DamageAmplifier amplifier = __instance.AllComps[i] as Comp_DamageAmplifier;

                    if (amplifier == null)
                    {
                        continue;
                    }

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
                __instance.weaponDamageMultiplier = __instance.weaponDamageMultiplier * multiplier + passiveOffset;
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

                ThingWithComps compThing = __instance.Instigator as ThingWithComps;

                if (compThing == null)
                {
                    return;
                }

                for (int i = compThing.AllComps.Count - 1; i >= 0; i--)
                {
                    Comp_DamageAmplifier amplifier = compThing.AllComps[i] as Comp_DamageAmplifier;

                    if (amplifier != null)
                    {
                        __result *= amplifier.DamageMultiplier;
                    }
                }

                Pawn pawn = __instance.Instigator as Pawn;

                if(pawn == null)
                {
                    return;
                }

                for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    Hediff hediff = pawn.health.hediffSet.hediffs[i];

                    if (hediff.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        __result *= hediff.def.GetModExtension<DamageAmplifierExtension>().damageMultiplier;
                    }

                    HediffWithComps compHediff = hediff as HediffWithComps;

                    if (compHediff == null)
                    {
                        continue;
                    }

                    for (int j = compHediff.comps.Count - 1; j >= 0; j--)
                    {
                        HediffComp_DamageAmplifier amplifier = compHediff.comps[j] as HediffComp_DamageAmplifier;

                        if (amplifier != null)
                        {
                            __result *= amplifier.DamageMultiplier;
                        }
                    }
                }

                if (pawn.apparel == null)
                {
                    return;
                }

                List<Apparel> wornApparel = pawn.apparel.WornApparel;
                for (int i = wornApparel.Count - 1; i >= 0; i--)
                {
                    Apparel apparel = wornApparel[i];

                    for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                    {
                        Comp_DamageAmplifier amplifier = apparel.AllComps[j] as Comp_DamageAmplifier;

                        if (amplifier == null)
                        {
                            continue;
                        }

                        __result *= amplifier.DamageMultiplier;
                    }

                    if (apparel.def.GetModExtension<DamageAmplifierExtension>() != null)
                    {
                        __result *= apparel.def.GetModExtension<DamageAmplifierExtension>().damageMultiplier;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(StunHandler), "Notify_DamageApplied")]
        public static class StunHandler_PreDamageApplied
        {
            static bool Prefix(StunHandler __instance, ref DamageInfo dinfo)
            {
                Pawn pawn = __instance.parent as Pawn;

                if (pawn == null)
                {
                    return true;
                }

                float modifier = 1f;
                float offset = 0f;

                for (int i = pawn.AllComps.Count - 1; i >= 0; i--)
                {
                    Comp_StunReduction stun = pawn.AllComps[i] as Comp_StunReduction;

                    if (stun != null)
                    {
                        if (stun.BlockStun(dinfo))
                        {
                            return false;
                        }

                        (float, float) result = stun.GetStunModifiers(dinfo);

                        offset += result.Item1;
                        modifier *= result.Item2;
                    }
                }

                for (int i = pawn.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    HediffWithComps hediff = pawn.health.hediffSet.hediffs[i] as HediffWithComps;

                    if (hediff == null)
                    {
                        continue;
                    }

                    for (int j = hediff.comps.Count - 1; j >= 0; j--)
                    {
                        HediffComp_Shield shield = hediff.comps[j] as HediffComp_Shield;

                        if (shield != null)
                        {
                            if (shield.BlockStun(ref dinfo))
                            {
                                return false;
                            }
                        }

                        HediffComp_StunReduction stun = hediff.comps[j] as HediffComp_StunReduction;

                        if (stun != null)
                        {
                            if (stun.BlockStun(dinfo))
                            {
                                return false;
                            }

                            (float, float) result = stun.GetStunModifiers(dinfo);

                            offset += result.Item1;
                            modifier *= result.Item2;
                        }
                    }
                }

                if (pawn.apparel != null)
                {
                    List<Apparel> wornApparel = pawn.apparel.WornApparel;
                    for (int i = wornApparel.Count - 1; i >= 0; i--)
                    {
                        Apparel apparel = wornApparel[i];
                        for (int j = apparel.AllComps.Count - 1; j >= 0; j--)
                        {
                            Comp_StunReduction stun = apparel.AllComps[j] as Comp_StunReduction;

                            if (stun != null)
                            {
                                if (stun.BlockStun(dinfo))
                                {
                                    return false;
                                }

                                (float, float) result = stun.GetStunModifiers(dinfo);

                                offset += result.Item1;
                                modifier *= result.Item2;
                            }
                        }
                    }
                }

                dinfo.SetAmount(dinfo.Amount * modifier + offset);

                return true;
            }
        }


        [HarmonyPatch(typeof(FoodUtility), "IsAcceptablePreyFor")]
        public static class FoodUtility_PreAcceptablePrey
        {
            static void Postfix(Pawn predator, Pawn prey, ref bool __result)
            {
                if (!__result)
                {
                    return;
                }

                MinPreySizeExtension ext = predator.def.GetModExtension<MinPreySizeExtension>();

                if (ext != null && prey.BodySize < ext.minPreyBodySize)
                {
                    __result = false;
                }
            }
        }

        [HarmonyPatch(typeof(Pawn), nameof(Pawn.DrawAt))]
        public static class Pawn_PostDrawAt
        {
            static void Postfix(Pawn __instance, Vector3 drawLoc)
            {
                for (int i = __instance.health.hediffSet.hediffs.Count - 1; i >= 0; i--)
                {
                    HediffWithComps hediff = __instance.health.hediffSet.hediffs[i] as HediffWithComps;

                    if (hediff == null)
                    {
                        continue;
                    }

                    for (int j = hediff.comps.Count - 1; j >= 0; j--)
                    {
                        HediffComp_Renderable renderable = hediff.comps[j] as HediffComp_Renderable;

                        if (renderable != null)
                        {
                            renderable.DrawAt(drawLoc);
                        }
                    }
                }

                if (__instance.apparel != null)
                {
                    List<Apparel> wornApparel = __instance.apparel.WornApparel;
                    for (int i = __instance.apparel.WornApparelCount - 1; i >= 0; i--)
                    {
                        Apparel apparel = wornApparel[i];

                        for (int j = apparel.comps.Count - 1; j >= 0; j--)
                        {
                            Comp_AdditionalApparelGraphics additionalGraphics = apparel.comps[j] as Comp_AdditionalApparelGraphics;

                            if (additionalGraphics != null)
                            {
                                additionalGraphics.DrawAt(drawLoc);
                            }
                        }
                    }
                }
            }
        }
    }
}
