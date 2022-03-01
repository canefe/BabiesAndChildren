using System;
using System.Reflection;
using Milk;
using HarmonyLib;
using Verse;
using RimWorld;

namespace BabiesAndChildren
{
    public class MCHarmonyPatches
    {

        private static Type mcUtilityType;
        public static void Patch()
        {
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("RimWorld.babies.and.children.mc." + nameof(MCHarmonyPatches));

            mcUtilityType = AccessTools.TypeByName("BabiesAndChildren.ChildrenUtility");

            MethodInfo original = AccessTools.Method(mcUtilityType, "CanBreastfeed");

            HarmonyMethod postfix = new HarmonyMethod(typeof(MCHarmonyPatches), nameof(CanBreastfeedPostfix));
            harmony.Patch(original, postfix: postfix);

            original = AccessTools.Method(mcUtilityType, "MCFallback");

            postfix = new HarmonyMethod(typeof(MCHarmonyPatches), nameof(MCFallbackPostfix));
            harmony.Patch(original, postfix: postfix);
        }

        private static void CanBreastfeedPostfix(ref bool __result, Pawn pawn)
        {
            if (pawn.TryGetComp<CompMilkableHuman>() != null)
            {
                if (pawn.TryGetComp<CompMilkableHuman>().Fullness > BnCMCSettings.feed)
                {
                    __result = true;
                    return;
                }
            }

            if (pawn.TryGetComp<CompHyperMilkableHuman>() != null)
            {
                if (pawn.TryGetComp<CompHyperMilkableHuman>().Fullness > BnCMCSettings.feed)
                {
                    __result = true;
                    return;
                }
            }

            __result = false;
        }
        //MCFallback()
        static AccessTools.FieldRef<HumanCompHasGatherableBodyResource, float> fullness = AccessTools.FieldRefAccess<HumanCompHasGatherableBodyResource, float>("fullness");
        private static void MCFallbackPostfix(Pawn pawn)
        {
            if (pawn.TryGetComp<CompMilkableHuman>() != null)
                fullness(pawn.TryGetComp<CompMilkableHuman>()) -= BnCMCSettings.feed;

            if (pawn.TryGetComp<CompHyperMilkableHuman>() != null)
                fullness(pawn.TryGetComp<CompHyperMilkableHuman>()) -= BnCMCSettings.feed;
        }

    }
}
