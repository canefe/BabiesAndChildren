using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using System.Collections.Generic;
using System;
using RimWorld;
using Verse;
using Verse.AI;
using RimWorld.BaseGen;

namespace BabiesAndChildren.Harmony
{
    [HarmonyPatch(typeof(JobGiver_SocialFighting), "TryGiveJob")]
    internal static class JobGiver_SocialFighting_TryGiveJob_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Pawn pawn, ref Job __result){
            Pawn other = ((MentalState_SocialFighting)pawn.MentalState).otherPawn;
            if (__result == null) return;
            
            if (AgeStages.IsYoungerThan(pawn, AgeStages.Child) && RaceUtility.PawnUsesChildren(pawn))
            {
                __result = null;
            }
            // Make sure kids don't start social fights with adults
            if (AgeStages.GetAgeStage(other) > AgeStages.Child && AgeStages.GetAgeStage(pawn) <= AgeStages.Child) {
                CLog.DevMessage("Child starting social fight with adult");
                // Adult will "start" the fight, following the code below
                other.interactions.StartSocialFight (pawn);
                __result = null;
            }

            // Make sure adults don't start social fights with kids (unless psychopaths)
            if (!AgeStages.IsOlderThan(other, AgeStages.Child) && 
                AgeStages.IsOlderThan(pawn, AgeStages.Child) && 
                !pawn.story.traits.HasTrait (TraitDefOf.Psychopath)) {
                
                CLog.DevMessage("Adult starting social fight with child");
                // If the pawn is not in a bad mood or is kind, they'll just tell them off
                if (pawn.story.traits.HasTrait (TraitDefOf.Kind) || pawn.needs.mood.CurInstantLevel > 0.45f || pawn.WorkTagIsDisabled(WorkTags.Violent)) {
                    //Log.Message ("Debug: Adult has decided to tell off the child");
                    JobDef chastise = BnCJobDefOf.ScoldChild;
                    __result = new Job (chastise, other);
                }
                // Otherwise the adult will smack the child around
                else if (other.health.summaryHealth.SummaryHealthPercent > 0.93f) {
                    //Log.Message ("Debug: Adult has decided to smack the child around, child health at " + other.health.summaryHealth.SummaryHealthPercent);
                    JobDef paddlin = BnCJobDefOf.DisciplineChild;
                    __result = new Job (paddlin, other);
                }

                pawn.MentalState.RecoverFromState ();
                __result = null;
            }
        }
    }

    //ApparelScoreGain
    [HarmonyPatch(typeof(JobGiver_OptimizeApparel), "ApparelScoreGain")]
    internal static class ApparelScoreGain_Patch
    {
        static void Postfix(ref Pawn pawn, ref Apparel ap, ref List<float> wornScoresCache, ref float __result)
        {
            string[] babyCloth = { "Apparel_Baby_Beanie", "Apparel_Baby_Onesie", "Apparel_Newborn_Onesie", "Apparel_Newborn_Beanie" };
            List<string> babyClothes = new List<string>(babyCloth);
            if (AgeStages.IsOlderThan(pawn, AgeStages.Toddler)) {
                if (babyClothes.Contains(ap.def.defName))
                {
                    __result = -1000f;
                }
            }
            if (AgeStages.IsYoungerThan(pawn, AgeStages.Child) && RaceUtility.PawnUsesChildren(pawn))
            {
                if (AgeStages.IsAgeStage(pawn, AgeStages.Toddler) && babyClothes.Contains(ap.def.defName) && ap.def.defName.Contains("Newborn")) __result = -1000f;
                if (!babyClothes.Contains(ap.def.defName))
                {
                    __result = -1000f;
                }
            }
        }
    }

    [HarmonyPatch(typeof(JoyGiver_InPrivateRoom), "TryGiveJobWhileInBed")]
    static class JoyGiver_InPrivateRoom_Patch // babies and toddlers no longer pray
    {
        static void Postfix(ref Pawn pawn, ref Job __result)
        {
            if (AgeStages.IsYoungerThan(pawn, AgeStages.Child) && RaceUtility.PawnUsesChildren(pawn))
            {
                __result = null;
            }

        }
    }

}