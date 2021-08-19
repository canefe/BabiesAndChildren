using System;
using System.Reflection;
using HarmonyLib;
using Verse;
using Verse.AI;
using RimWorld;
using BabiesAndChildren.api;

namespace BabiesAndChildren.Harmony
{
    public static class DubsBadHygienePatches
    {
        private static Type workGiver_washPatientType;
        public static void Patch()
        {
            HarmonyLib.Harmony harmony =
                new HarmonyLib.Harmony("RimWorld.babies.and.children." + nameof(DubsBadHygienePatches));

            workGiver_washPatientType = AccessTools.TypeByName("WorkGiver_washPatient");

            MethodInfo original = AccessTools.Method(workGiver_washPatientType, "ShouldBeWashedBySomeone");

            HarmonyMethod postfix = new HarmonyMethod(typeof(DubsBadHygienePatches), nameof(ShouldBeWashedBySomeonePostfix));

            harmony.Patch(original, postfix: postfix);
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("Need_Bladder"), "NeedInterval"), postfix: new HarmonyMethod(typeof(DubsBadHygienePatches), nameof(NeedIntervalPostfix)));
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("JobGiver_HaveWash"), "GetPriority"), postfix: new HarmonyMethod(typeof(DubsBadHygienePatches), nameof(GetPriorityPostfix)));
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("WorkGiver_washPatient"), "HasJobOnThing"), postfix: new HarmonyMethod(typeof(DubsBadHygienePatches), nameof(HasJobOnThingPostfix)));//HasJobOnThing
        }

        private static void ShouldBeWashedBySomeonePostfix(Pawn pawn, ref bool __result) 
        {
            if (!__result && ChildrenUtility.ShouldBeCaredFor(pawn))
            {
                __result = true;
            }
        }
        private static void NeedIntervalPostfix(Need __instance, Pawn ___pawn)
        {
            if (__instance.CurLevel < 0.15 && AgeStages.IsAgeStage(___pawn, AgeStages.Toddler))
            {
                __instance.CurLevel = 1f;
                Thing thing;
                GenThing.TryDropAndSetForbidden(ThingMaker.MakeThing(ThingDef.Named("BedPan"), null), ___pawn.Position, ___pawn.Map, ThingPlaceMode.Near, out thing, false);
            }
        }
        private static void GetPriorityPostfix(Pawn pawn, ref float __result)
        {
            if (AgeStages.IsAgeStage(pawn, AgeStages.Toddler))
            {
                __result = 0f;
            }
        }
        private static void HasJobOnThingPostfix(Pawn pawn, Thing t, ref bool __result)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || pawn2 == pawn)
            {
                __result = false;
            }
            if (AgeStages.IsAgeStage(pawn2, AgeStages.Toddler) && pawn2.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamed("Hygiene")).CurLevel < 0.3f && pawn.CanReserve(pawn2, 1, -1, null, false))
            {
                __result = true;
            }
        }
    }
}