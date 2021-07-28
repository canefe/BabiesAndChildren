using BabiesAndChildren.Tools;
using HarmonyLib;
using Verse;

namespace BabiesAndChildren.Harmony
{
    [HarmonyPatch(typeof(PawnGraphicSet), "ResolveAllGraphics")]
    internal static class PawnGraphicSet_ResolveAllGraphics_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref PawnGraphicSet __instance)
        {
            Pawn pawn = __instance.pawn;
            PawnGraphicSet _this = __instance;
            if (RaceUtility.PawnUsesChildren(pawn))
            {
                GraphicTools.ResolveAgeGraphics(__instance);
                LongEventHandler.ExecuteWhenFinished(delegate
                {
                    _this.ResolveApparelGraphics();
                });
            }
        }
    }
}