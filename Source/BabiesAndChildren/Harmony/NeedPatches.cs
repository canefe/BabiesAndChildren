using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BabiesAndChildren.Harmony {

    /// <summary>
    /// Babies should not develop a tolerance for social joy since this is currently their only available joy source.
    /// </summary>
    [HarmonyPatch(typeof(Need_Joy), "GainJoy")]
    internal static class Need_Joy_GainJoy_Patch {
        [HarmonyPostfix]
        static void GainJoy_Patch(Need_Joy __instance, JoyKindDef joyKind, Pawn ___pawn)
        {
            if (!RaceUtility.PawnUsesChildren(___pawn) || 
                joyKind != JoyKindDefOf.Social ||
                AgeStages.IsOlderThan(___pawn, AgeStages.Toddler))
                return;
            
            DefMap<JoyKindDef, float> tolerances = Traverse.Create(__instance).Field("tolerances").Field("tolerances").GetValue<DefMap<JoyKindDef, float>>();
            DefMap<JoyKindDef, bool> bored = Traverse.Create(__instance).Field("tolerances").Field("bored").GetValue<DefMap<JoyKindDef, bool>>();
            tolerances[JoyKindDefOf.Social] = 0;
            bored[JoyKindDefOf.Social] = false;
        }
    }
}