using System;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using Verse;
using RimWorld;
using RimWorld.QuestGen;

namespace BabiesAndChildren.Harmony
{

    [HarmonyPatch(typeof(PawnGenerator), "RedressPawn")]
    internal static class PawnGenerator_GenerateBodyType_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Pawn pawn)
        {
            if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsYoungerThan(pawn, AgeStages.Teenager))
            {
                StoryUtility.ChangeBodyType(pawn);
                pawn.style.beardDef = BeardDefOf.NoBeard;
                pawn.style.bodyTattoo = TattooDefOf.NoTattoo_Body;
                pawn.style.faceTattoo = TattooDefOf.NoTattoo_Face;
            }
        }
    }

    [HarmonyPatch(typeof(PawnGenerator), "GenerateNewPawnInternal")]
    internal static class PawnGenerator_GeneratePawn_Patch
    {
        static void Postfix(ref PawnGenerationRequest request, ref Pawn __result)
        {
            if (__result != null && RaceUtility.PawnUsesChildren(__result) && AgeStages.IsYoungerThan(__result, AgeStages.Teenager))
            {
                StoryUtility.ChangeBodyType(__result);
                __result.style.beardDef = BeardDefOf.NoBeard;
                __result.style.bodyTattoo = TattooDefOf.NoTattoo_Body;
                __result.style.faceTattoo = TattooDefOf.NoTattoo_Face;
            }
        }
    }
    
}