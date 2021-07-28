using System;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using Verse;

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
            }
        }
    }
    
}