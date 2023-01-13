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
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        public static Harmony harmonyInstance;

        public static bool apparelPatched = false;
        public static bool drawPatched = false;
        public static bool damagesPatched = false;

        static HarmonyPatches()
        {
            harmonyInstance = new Harmony(id: "rimworld.smartkar.athenaframework.main");
            harmonyInstance.PatchAll();
        }
    }
}
