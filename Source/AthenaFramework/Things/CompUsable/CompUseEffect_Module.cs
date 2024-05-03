using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace AthenaFramework
{
    public class CompUseEffect_Module : CompUseEffect, IArmored, IStatModifier
    {
        private new CompProperties_UseEffectModule Props => props as CompProperties_UseEffectModule;

        public string usedSlot;
        public List<ThingComp> linkedComps = new List<ThingComp>();
        public CompModular ownerComp;
        public ThingWithComps ownerThing;

        public Dictionary<StatDef, float> equippedStatOffsets = new Dictionary<StatDef, float>();
        public Dictionary<StatDef, float> statOffsets = new Dictionary<StatDef, float>();
        public Dictionary<StatDef, float> statFactors = new Dictionary<StatDef, float>();
        public virtual List<string> Slots
        {
            get
            {
                return Props.slotIDs;
            }
        }

        public virtual float RequiredCapacity
        {
            get
            {
                return Props.requiredCapacity;
            }
        }

        public virtual List<string> ExcludeIDs
        {
            get
            {
                return Props.excludeIDs;
            }
        }

        public virtual List<ApparelGraphicPackage> GetGraphics
        {
            get
            {
                return Props.additionalGraphics;
            }
        }

        public virtual bool Ejectable
        {
            get
            {
                return Props.ejectable;
            }
        }

        public virtual ArmorMode CurrentArmorMode
        {
            get
            {
                return Props.armorMode;
            }
        }

        public virtual CompModular OwnerComp
        {
            get
            {
                if (ownerComp != null)
                {
                    return ownerComp;
                }

                if (ownerThing == null)
                {
                    return null;
                }

                ownerComp = ownerThing.TryGetComp<CompModular>();

                return ownerComp;
            }
        }

        public virtual ModuleSlotPackage GetSlot
        {
            get
            {
                if (OwnerComp == null)
                {
                    return null;
                }

                for (int i = OwnerComp.GetAllSlots.Count - 1; i >= 0; i--)
                {
                    ModuleSlotPackage slot = OwnerComp.GetAllSlots[i];

                    if (slot.slotID == usedSlot)
                    {
                        return slot;
                    }
                }

                return null;
            }
        }

        public override void DoEffect(Pawn user)
        {
            if (OwnerComp == null || OwnerComp.parent == null)
            {
                ownerThing = null;
                ownerComp = null;
                return;
            }

            OwnerComp.InstallModule(parent);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref ownerThing, "ownerThing");
            Scribe_Values.Look(ref usedSlot, "usedSlot");
        }

        public override AcceptanceReport CanBeUsedBy(Pawn p)
        {
            AcceptanceReport result = base.CanBeUsedBy(p);
            if (!result.Accepted)
            {
                return result;
            }

            for (int i = p.equipment.equipment.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = p.equipment.equipment[i];

                for (int j = thing.AllComps.Count - 1; j >= 0; j--)
                {
                    CompModular comp = thing.AllComps[j] as CompModular;
                    if (comp != null && comp.GetOpenSlots(this).Count > 0)
                    {
                        return true;
                    }
                }
            }

            List<Apparel> wornApparel = p.apparel.WornApparel;
            for (int i = wornApparel.Count - 1; i >= 0; i--)
            {
                ThingWithComps thing = wornApparel[i];

                for (int j = thing.AllComps.Count - 1; j >= 0; j--)
                {
                    CompModular comp = thing.AllComps[j] as CompModular;
                    if (comp != null && comp.GetOpenSlots(this).Count > 0)
                    {

                        if (Props.prerequisites != null && !Props.prerequisites.ValidPawn(p))
                        {
                            continue;
                        }

                        return true;
                    }
                }
            }

            return "Cannot apply: No compatible slots availible.";
        }

        public virtual bool Install(CompModular comp)
        {
            if (CurrentArmorMode != ArmorMode.None)
            {
                AthenaCache.AddCache(this, ref AthenaCache.armorCache, comp.parent.thingIDNumber);
            }

            AthenaCache.AddCache(this, ref AthenaCache.statmodCache, comp.parent.thingIDNumber);

            if (Props.installSound != null)
            {
                Props.installSound.PlayOneShot(SoundInfo.InMap(comp.parent, MaintenanceType.None));
            }

            if (Props.comps != null)
            {
                for (int i = Props.comps.Count - 1; i >= 0; i--)
                {
                    ThingComp thingComp = null;
                    try
                    {
                        thingComp = (ThingComp)Activator.CreateInstance(Props.comps[i].compClass);
                        thingComp.parent = comp.parent;
                        comp.parent.comps.Add(thingComp);
                        linkedComps.Add(thingComp);
                        thingComp.Initialize(Props.comps[i]);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Modular ThingComp could not instantiate or initialize a ThingComp: " + ex);
                        comp.parent.comps.Remove(thingComp);
                    }
                }
            }

            return true;
        }

        public virtual bool Remove(CompModular comp)
        {
            if (!Props.ejectable)
            {
                return false;
            }

            if (Props.ejectSound != null)
            {
                Props.ejectSound.PlayOneShot(SoundInfo.InMap(comp.parent, MaintenanceType.None));
            }

            if (CurrentArmorMode  != ArmorMode.None)
            {
                AthenaCache.RemoveCache(this, AthenaCache.armorCache, comp.parent.thingIDNumber);
            }

            AthenaCache.RemoveCache(this, AthenaCache.statmodCache, comp.parent.thingIDNumber);

            for (int i = linkedComps.Count - 1; i >= 0; i--)
            {
                comp.parent.comps.Remove(linkedComps[i]);
                linkedComps.RemoveAt(i);
            }

            ownerComp = null;
            ownerThing = null;

            return true;
        }

        public virtual void PostInit(CompModular comp)
        {
            if (CurrentArmorMode != ArmorMode.None)
            {
                AthenaCache.AddCache(this, ref AthenaCache.armorCache, comp.parent.thingIDNumber);
            }

            AthenaCache.AddCache(this, ref AthenaCache.statmodCache, comp.parent.thingIDNumber);

            if (Props.comps == null)
            {
                return;
            }

            for (int i = Props.comps.Count - 1; i >= 0; i--)
            {
                ThingComp thingComp = null;
                try
                {
                    thingComp = (ThingComp)Activator.CreateInstance(Props.comps[i].compClass);
                    thingComp.parent = comp.parent;
                    comp.parent.comps.Add(thingComp);
                    linkedComps.Add(thingComp);
                    thingComp.Initialize(Props.comps[i]);
                }
                catch (Exception ex)
                {
                    Log.Error("Modular ThingComp could not instantiate or initialize a ThingComp: " + ex);
                    comp.parent.comps.Remove(thingComp);
                }
            }
        }

        public virtual void GearStatOffset(StatDef stat, ref float result)
        {
            if (!equippedStatOffsets.ContainsKey(stat))
            {
                return;
            }

            result += equippedStatOffsets[stat];
        }

        public virtual bool GearAffectsStat(StatDef stat)
        {
            return equippedStatOffsets.ContainsKey(stat);
        }

        public virtual void GetValueOffsets(StatWorker worker, StatRequest req, bool applyPostProcess, ref float result)
        {
            if (!statOffsets.ContainsKey(worker.stat))
            {
                return;
            }

            result += statOffsets[worker.stat];
        }

        public virtual void GetValueFactors(StatWorker worker, StatRequest req, bool applyPostProcess, ref float result)
        {
            if (!statFactors.ContainsKey(worker.stat))
            {
                return;
            }

            result *= statFactors[worker.stat];
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);

            for (int i = Props.equippedStatOffsets.Count - 1; i >= 0; i--)
            {
                StatModifier statmod = Props.equippedStatOffsets[i];
                equippedStatOffsets[statmod.stat] = statmod.value;
            }

            for (int i = Props.statOffsets.Count - 1; i >= 0; i--)
            {
                StatModifier statmod = Props.statOffsets[i];
                statOffsets[statmod.stat] = statmod.value;
            }

            for (int i = Props.statFactors.Count - 1; i >= 0; i--)
            {
                StatModifier statmod = Props.statFactors[i];
                statFactors[statmod.stat] = statmod.value;
            }
        }

        public virtual bool PreProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount)
        {
            if (CurrentArmorMode == ArmorMode.None || !CoversPart(part))
            {
                return true;
            }

            if (CurrentArmorMode == ArmorMode.Additional)
            {
                for (int j = Props.armorStats.Count - 1; j >= 0; j--)
                {
                    StatModifier localStat = Props.armorStats[j];

                    if (localStat.stat == stat)
                    {
                        armorRating += localStat.value;
                        break;
                    }
                }

                return true;
            }

            if (CurrentArmorMode != ArmorMode.Exclusive)
            {
                return true;
            }

            float localArmorRating = 0f;

            for (int j = Props.armorStats.Count - 1; j >= 0; j--)
            {
                StatModifier localStat = Props.armorStats[j];

                if (localStat.stat == stat)
                {
                    localArmorRating = localStat.value;
                    break;
                }
            }

            if (Props.metallicBlock)
            {
                metalArmor = true;
            }

            float blockLeft = Mathf.Max(localArmorRating - armorPenetration, 0f);
            float randomValue = Rand.Value;

            if (randomValue < blockLeft * 0.5)
            {
                amount = 0f;
                return false;
            }

            if (randomValue < blockLeft)
            {
                amount /= 2;

                if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    damageDef = DamageDefOf.Blunt;
                }
            }

            return false;
        }

        public virtual void PostProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount)
        {
            if (CurrentArmorMode != ArmorMode.Ontop || !CoversPart(part))
            {
                return;
            }

            float localArmorRating = 0f;

            for (int j = Props.armorStats.Count - 1; j >= 0; j--)
            {
                StatModifier localStat = Props.armorStats[j];

                if (localStat.stat == stat)
                {
                    localArmorRating = localStat.value;
                    break;
                }
            }

            if (Props.metallicBlock)
            {
                metalArmor = true;
            }

            float blockLeft = Mathf.Max(localArmorRating - armorPenetration, 0f);
            float randomValue = Rand.Value;

            if (randomValue < blockLeft * 0.5)
            {
                amount = 0f;
                return;
            }

            if (randomValue < blockLeft)
            {
                amount /= 2;

                if (damageDef.armorCategory == DamageArmorCategoryDefOf.Sharp)
                {
                    damageDef = DamageDefOf.Blunt;
                }
            }
        }

        public virtual bool CoversBodypart(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, bool defaultCovered, out bool forceCover, out bool forceUncover)
        {
            forceCover = false;
            forceUncover = false;

            if (CurrentArmorMode == ArmorMode.None)
            {
                return false;
            }

            return CoversPart(part);
        }

        public virtual bool CoversPart(BodyPartRecord part)
        {
            if (Props.coveredParts != null && Props.coveredParts.Intersect(part.groups).Count() > 0)
            {
                return true;
            }

            return false;
        }
    }

    public class CompProperties_UseEffectModule : CompProperties_UseEffect
    {
        public CompProperties_UseEffectModule()
        {
            this.compClass = typeof(CompUseEffect_Module);
        }

        // List of slot IDs that this module can be installed into
        public List<string> slotIDs;
        // Amount of slot capacity that this module takes
        public float requiredCapacity = 0;
        // List of IDs that this module is incompatible with
        public List<string> excludeIDs;
        // If this module can be ejected
        public bool ejectable = true;
        // Sound that is played when the module is installed
        public SoundDef installSound;
        // Sound that is played when the module is ejected
        public SoundDef ejectSound;
        // List of comps that are applied when this module is attached
        public List<CompProperties> comps;
        // List of additional graphics that are applied if the parent pawn has an IRenderable that accepts apparel graphic packages linked to it
        public List<ApparelGraphicPackage> additionalGraphics;
        // List of modified stats
        public List<StatModifier> equippedStatOffsets = new List<StatModifier>();
        public List<StatModifier> statOffsets = new List<StatModifier>();
        public List<StatModifier> statFactors = new List<StatModifier>();
        // List of areas that this module covers
        public List<BodyPartGroupDef> coveredParts;
        // How armor should behave
        public ArmorMode armorMode = ArmorMode.None;
        // Stats for armor
        public List<StatModifier> armorStats;
        // If block VFX should be metallic
        public bool metallicBlock = false;
        // Prerequisite properties for the module, with the module only be able to be installed if the owner pawn fits the criteria. Assign the values in the props as you would normally
        // Does nothing if the owner isn't a pawn
        public CompProperties_PrerequisiteEquippable prerequisites;
    }

    public enum ArmorMode
    {
        None, // Armor calculations are disabled
        Additional, // Armor stats are applied to the original armor before armor calculations
        Ontop, // Acts as a second layer of armor, potentially halving the damage a second time
        Exclusive // Hijacks the armor calculations, preventing other modules or main apparel from applying their armor
    }
}
