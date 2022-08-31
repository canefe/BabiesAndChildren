using System;
using System.Reflection;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;

namespace BabiesAndChildren.Harmony
{
    public class MedPodPatches
    {
        private static Type MedPodType;
        public static void Patch()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("RimWorld.babies.and.children." + nameof(MedPodPatches));

            MedPodType = AccessTools.TypeByName("MedPod.WorkGiver_DoctorRescueToMedPod");


            MethodInfo original = AccessTools.Method(MedPodType, "HasJobOnThing");

                

            HarmonyMethod postfix = new HarmonyMethod(typeof(MedPodPatches), nameof(HasJobOnThingPostfix));
            harmony.Patch(original, postfix: postfix);
        }

        private static void HasJobOnThingPostfix(ref bool __result, Pawn pawn, Thing t)
        {
            Pawn victim = t as Pawn;
            if (victim != null && victim.Downed && RaceUtility.PawnUsesChildren(victim) && AgeStages.IsAgeStage(victim, AgeStages.Baby))
            {
                __result = false;
            }
        }
    }
}