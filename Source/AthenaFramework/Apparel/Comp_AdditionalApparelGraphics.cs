using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace AthenaFramework
{
    [StaticConstructorOnStartup]
    public class Comp_AdditionalApparelGraphics : ThingComp, IRenderable, IColorSelector
    {
        private CompProperties_AdditionalApparelGraphics Props => props as CompProperties_AdditionalApparelGraphics;
        private Apparel Apparel => parent as Apparel;
        private Pawn Pawn => Apparel.Wearer as Pawn;

        public static readonly Texture2D cachedPaletteTex = ContentFinder<Texture2D>.Get("UI/Gizmos/ColorPalette");
        public Color primaryColor = Color.white;
        public Color secondaryColor = Color.white;
        public bool? usePrimary;
        public bool? useSecondary;
        public Command_Action paletteAction;
        public List<ApparelGraphicPackage> additionalGraphics;

        public Mote attachedMote;
        public Effecter attachedEffecter;

        public virtual Color PrimaryColor
        {
            get
            {
                return primaryColor;
            }

            set
            {
                primaryColor = value;
            }
        }

        public virtual Color SecondaryColor
        {
            get
            {
                return secondaryColor;
            }

            set
            {
                secondaryColor = value;
            }
        }

        public virtual bool UseSecondary
        {
            get
            {
                return (bool)useSecondary;
            }
        }

        public virtual void RecacheGraphicData()
        {
            if (Props.additionalGraphics == null)
            {
                additionalGraphics = new List<ApparelGraphicPackage>();
            }
            else
            {
                additionalGraphics = new List<ApparelGraphicPackage>(Props.additionalGraphics);
            }

            for (int i = parent.comps.Count - 1; i >= 0; i--)
            {
                IEquippableGraphicGiver modular = parent.comps[i] as IEquippableGraphicGiver;

                if (modular != null)
                {
                    additionalGraphics = additionalGraphics.Concat(modular.GetAdditionalGraphics).ToList();
                }
            }

            usePrimary = false;
            useSecondary = false;

            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                ApparelGraphicPackage package = additionalGraphics[i];

                if (package.firstMask == ApparelPackageColor.PrimaryColor || package.secondMask == ApparelPackageColor.PrimaryColor)
                {
                    usePrimary = true;
                }

                if (package.firstMask == ApparelPackageColor.SecondaryColor || package.secondMask == ApparelPackageColor.SecondaryColor)
                {
                    usePrimary = true;
                    useSecondary = true;
                    break;
                }
            }
        }

        public virtual void DrawAt(Vector3 drawPos, BodyTypeDef bodyType)
        {
            for (int i = additionalGraphics.Count - 1; i >= 0; i--)
            {
                ApparelGraphicPackage package = additionalGraphics[i];
                Vector3 offset = new Vector3();

                if (package.offsets != null)
                {
                    if (package.offsets.Count == 4)
                    {
                        offset = package.offsets[Pawn.Rotation.AsInt];
                    }
                    else
                    {
                        offset = package.offsets[0];
                    }
                }

                package.GetGraphic(Apparel, bodyType).Draw(drawPos + offset, Pawn.Rotation, Pawn);
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            if (usePrimary == null || useSecondary == null)
            {
                RecacheGraphicData();
            }

            if (!(bool)usePrimary)
            {
                yield break;
            }

            if (paletteAction == null)
            {
                paletteAction = new Command_Action();
                paletteAction.defaultLabel = "Change colors for " + parent.LabelCap;
                paletteAction.icon = cachedPaletteTex;
                paletteAction.action = delegate ()
                {
                    Find.WindowStack.Add(new Dialog_ColorPalette(this));
                };
            }

            yield return paletteAction;
            yield break;
        }

        public override void CompTick()
        {
            base.CompTick();

            if (Props.attachedMoteDef != null)
            {
                if (attachedMote == null || attachedMote.Destroyed)
                {
                    attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
                }

                attachedMote.Maintain();
            }

            if (Props.attachedEffecterDef != null)
            {
                if (attachedEffecter == null)
                {
                    attachedEffecter = Props.attachedEffecterDef.SpawnAttached(Pawn, Pawn.Map);
                }

                attachedEffecter.EffectTick(Pawn, Pawn);
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            AthenaCache.AddCache(this, AthenaCache.renderCache, pawn.thingIDNumber);

            if (Props.attachedMoteDef != null)
            {
                attachedMote = MoteMaker.MakeAttachedOverlay(Pawn, Props.attachedMoteDef, Props.attachedMoteOffset, Props.attachedMoteScale);
            }

            if (Props.attachedEffecterDef != null)
            {
                attachedEffecter = Props.attachedEffecterDef.SpawnAttached(Pawn, Pawn.Map);
            }

            RecacheGraphicData();
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            AthenaCache.RemoveCache(this, AthenaCache.renderCache, pawn.thingIDNumber);

            if (attachedMote != null)
            {
                attachedMote.Destroy();
                attachedMote = null;
            }

            if (attachedEffecter != null)
            {
                attachedEffecter = null;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref primaryColor, "primaryColor");
            Scribe_Values.Look(ref secondaryColor, "secondaryColor");
        }
    }

    public class CompProperties_AdditionalApparelGraphics : CompProperties
    {
        public CompProperties_AdditionalApparelGraphics()
        {
            this.compClass = typeof(Comp_AdditionalApparelGraphics);
        }

        // Mote attached to the equipment piece
        public ThingDef attachedMoteDef;
        // Effecter attached to the equipment piece
        public EffecterDef attachedEffecterDef;
        // Offset of the attached mote
        public Vector3 attachedMoteOffset = new Vector3();
        // Scale of the attached mote
        public float attachedMoteScale = 1f;
        // Additional graphic layers with precise controls
        public List<ApparelGraphicPackage> additionalGraphics = new List<ApparelGraphicPackage>();
    }
}
