using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using static HarmonyLib.Code;
using RimWorld;
using Verse;
using UnityEngine;

namespace AthenaFramework
{
    public class AthenaFrameworkPatches : Mod
    {
        public Harmony harmonyInstance;

        public AthenaFrameworkPatches(ModContentPack content) : base(content)
        {
            harmonyInstance = new Harmony(id: "rimworld.smartkar.athenaframework.main");
            harmonyInstance.PatchAll();
        }
    }

    public static class AthenaCache
    {
        public static Dictionary<int, List<IRenderable>> renderCache;
        public static Dictionary<int, List<IBodyModifier>> bodyCache;
        public static Dictionary<int, List<IArmored>> armorCache;
        public static Dictionary<int, List<IDamageResponse>> responderCache;
        public static Dictionary<int, List<IDamageModifier>> damageCache; 
        public static Dictionary<int, List<IStatModifier>> statmodCache;
        public static Dictionary<int, List<IProjectile>> projectileCache;
        public static Dictionary<int, List<IPreventEquip>> equipCache;
        public static Dictionary<int, List<IFloatMenu>> menuCache;

        public static void AddCache<T>(T elem, ref Dictionary<int, List<T>> cacheList, int id)
        {
            if (cacheList == null)
            {
                Reset();
            }

            if (cacheList.TryGetValue(id, out List<T> mods))
            {
                if (!mods.Contains(elem))
                {
                    mods.Add(elem);
                }
            }
            else
            {
                cacheList[id] = new List<T>() { elem };
            }
        }

        public static void RemoveCache<T>(T elem, Dictionary<int, List<T>> cacheList, int id)
        {
            if(!cacheList.TryGetValue(id, out List<T> mods))
            {
                return;
            }

            mods.Remove(elem);

            if (mods.Count == 0)
            {
                cacheList.Remove(id);
            }
        }

        public static void Reset()
        {
            AthenaCache.renderCache = new Dictionary<int, List<IRenderable>>();
            AthenaCache.bodyCache = new Dictionary<int, List<IBodyModifier>>();
            AthenaCache.armorCache = new Dictionary<int, List<IArmored>>();
            AthenaCache.responderCache = new Dictionary<int, List<IDamageResponse>>();
            AthenaCache.damageCache = new Dictionary<int, List<IDamageModifier>>();
            AthenaCache.statmodCache = new Dictionary<int, List<IStatModifier>>();
            AthenaCache.projectileCache = new Dictionary<int, List<IProjectile>>();
            AthenaCache.equipCache = new Dictionary<int, List<IPreventEquip>>();
            AthenaCache.menuCache = new Dictionary<int, List<IFloatMenu>>();
        }
    }
}
