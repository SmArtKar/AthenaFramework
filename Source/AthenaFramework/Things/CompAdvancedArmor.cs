using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace AthenaFramework
{
    public class CompAdvancedArmor : ThingComp, IArmored
    {
        private CompProperties_AdvancedArmor Props => props as CompProperties_AdvancedArmor;

        public virtual bool CoversBodypart(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, bool defaultCovered, out bool forceCover, out bool forceUncover)
        {
            forceCover = false;
            forceUncover = false;

            return false;
        }

        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            AthenaCache.AddCache(this,  ref AthenaCache.armorCache, parent.thingIDNumber);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            AthenaCache.RemoveCache(this, AthenaCache.armorCache, parent.thingIDNumber);
        }

        public void PostProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount) { }

        public bool PreProcessArmor(ref float amount, float armorPenetration, StatDef stat, ref float armorRating, BodyPartRecord part, ref DamageDef damageDef, Pawn pawn, ref bool metalArmor, DamageDef originalDamageDef, float originalAmount)
        {
            for (int i = Props.armorPackages.Count - 1; i >= 0; i--)
            {
                ArmorPackage package = Props.armorPackages[i];

                if (package.partGroups != null && package.partGroups.Intersect(part.groups).Count() == 0)
                {
                    continue;
                }

                for (int j = package.armorModifiers.Count - 1; j >= 0; j--)
                {
                    StatModifier localStat = package.armorModifiers[j];

                    if (localStat.stat == stat)
                    {
                        armorRating += localStat.value;
                        break;
                    }
                }

                for (int j = package.defArmors.Count - 1; j >= 0; j--)
                {
                    DamageDefArmor localStat = package.defArmors[j];

                    if (localStat.damageDef == damageDef)
                    {
                        armorRating += localStat.value;
                        break;
                    }
                }
            }

            return true;
        }
    }

    public class CompProperties_AdvancedArmor : CompProperties
    {
        public CompProperties_AdvancedArmor()
        {
            this.compClass = typeof(CompAdvancedArmor);
        }

        public List<ArmorPackage> armorPackages = new List<ArmorPackage>();
    }

    public class ArmorPackage
    {
        public List<BodyPartGroupDef> partGroups;
        public List<StatModifier> armorModifiers = new List<StatModifier>();
        public List<DamageDefArmor> defArmors = new List<DamageDefArmor>();
    }

    public class DamageDefArmor
    {
        public DamageDef damageDef;
        public float value;

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "damageDef", xmlRoot.Name);
            value = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
        }
    }
}
