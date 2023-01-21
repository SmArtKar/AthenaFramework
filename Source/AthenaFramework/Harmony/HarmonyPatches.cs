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
}
