using System;
using System.Reflection;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using Verse;

namespace BabiesAndChildren.Harmony
{
    public class DressPatientsPatches
    {
        private static Type dressPatientUtilityType;
        public static void Patch()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("RimWorld.babies.and.children." + nameof(DressPatientsPatches));

            dressPatientUtilityType = AccessTools.TypeByName("DressPatient.DressPatientUtility");

            MethodInfo original = AccessTools.Method(dressPatientUtilityType, "IsPatient");

            HarmonyMethod postfix = new HarmonyMethod(typeof(DressPatientsPatches), nameof(IsPatientPostfix));
            harmony.Patch(original, postfix: postfix);
        }

        private static void IsPatientPostfix(ref bool __result, Pawn pawn)
        {
            if (!__result && RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsAgeStage(pawn, AgeStages.Baby))
                __result = true;
        }
    }
}