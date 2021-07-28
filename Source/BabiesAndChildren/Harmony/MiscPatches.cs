using System.Collections.Generic;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using RimWorld;
using Verse;

namespace BabiesAndChildren.Harmony
{
    [HarmonyPatch(typeof(EquipmentUtility), "CanEquip", 
        new[] { 
            typeof(Thing), 
            typeof(Pawn), 
            typeof(string),
            typeof(bool)
        }
        , new[]{
            ArgumentType.Normal, 
            ArgumentType.Normal, 
            ArgumentType.Out,
            ArgumentType.Normal
        })]
    internal static class EquipmentUtility_CanEquip_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref bool __result, Thing thing, Pawn pawn, out string cantReason, bool checkBonded)
        {
            
            cantReason = null;
            
            if(!__result)
                return;
            
            if (thing.def.thingSetMakerTags != null)
            {
                // prevent non-children from equipping toys
                if (thing.def.thingSetMakerTags.Contains("Toy") && !AgeStages.IsAgeStage(pawn, AgeStages.Child))
                {
                    cantReason = "OnlyChildrenCanEquip".Translate();
                    __result = false;
                }
                
                // prevent non-toddlers from equipping toddler clothes
                if (thing.def.thingSetMakerTags.Contains("BabyGear") && 
                         !AgeStages.IsAgeStage(pawn, AgeStages.Toddler) )
                {
                    //this message probably isn't right but meh
                    cantReason = "OnlyForUprightToddler".Translate();
                    __result = false;
                }
                
                // prevent non-babies from equipping baby clothes
                if (thing.def.thingSetMakerTags.Contains("BabyGear1") &&
                         !AgeStages.IsAgeStage(pawn, AgeStages.Baby))
                {
                    //probably need a specialized message but meh
                    cantReason = "OnlyChildrenCanEquip".Translate();
                    __result = false;
                }
                
                
            }
            // prevent babies and toddlers from equipping adult clothes
            if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsYoungerThan(pawn, AgeStages.Child))
            {
                if (thing.def.thingSetMakerTags == null || !thing.def.thingSetMakerTags.Contains("BabyGear") || !thing.def.thingSetMakerTags.Contains("BabyGear1"))
                {
                    cantReason = "BabyCantEquipNormal".Translate();
                    __result = false;
                }
            }
        }
    }
    [HarmonyPatch(typeof(HealthAIUtility), "ShouldSeekMedicalRestUrgent")]
    internal static class HealthAIUtility_ShouldSeekMedicalRestUrgent_Patch
    {
        /// <summary>
        /// Combined with RestUtility patches, this patch ensures that a baby pawn who is not
        /// truly in need of medical attention will be moved to a crib instead of "rescued"
        /// to a hospital bed. 
        /// </summary>
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, ref bool __result)
        {
            if (__result && 
                RaceUtility.ThingUsesChildren(pawn) && 
                ChildrenUtility.ShouldUseCrib(pawn) && 
                pawn.Downed && !(pawn.health.HasHediffsNeedingTend() || HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn)))
            {
                __result = false;
            }
        }
    }
    //original => WardenFeedUtility.ShouldBeFed || FeedPatientUtility.ShouldBeFed

    [HarmonyPatch(typeof(FoodUtility), "ShouldBeFedBySomeone")]
    internal static class FoodUtility_ShouldBeFedBySomeone_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, ref bool __result)
        {
            if (!__result && ChildrenUtility.ShouldBeFed(pawn))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(WardenFeedUtility), "ShouldBeFed")]
    internal static class WardenFeedUtility_ShouldBeFed_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result && p.IsPrisonerOfColony && ChildrenUtility.ShouldBeFed(p))
            {
                __result = true;
            }
        }
        
    }

    [HarmonyPatch(typeof(FeedPatientUtility), "ShouldBeFed")]
    internal static class FeedPatientUtility_ShouldBeFed_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Pawn p, ref bool __result)
        {
            if (!__result && ChildrenUtility.ShouldBeFed(p))
            {
                __result = true;
            }
            
        }
    }

    // Implements thoughts blacklist for non-adult pawns
    [HarmonyPatch(typeof(ThoughtUtility), "CanGetThought")]
    internal static class ThoughtUtility_CanGetThought_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref bool __result, ref Pawn pawn, ref ThoughtDef def)
        {
            if (!RaceUtility.PawnUsesChildren(pawn))
            {
                return;
            }
            __result = __result && !Thoughts.IsBlacklisted(def, AgeStages.GetAgeStage(pawn));
        }
    }

    [HarmonyPatch(typeof(Alert_ColonistsIdle), "IdleColonists", MethodType.Getter)]
    internal static class Alert_ColonistsIdle_IdleColonists_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref List<Pawn> __result)
        {
            __result.RemoveAll(pawn => AgeStages.IsYoungerThan(pawn, AgeStages.Teenager));
        }
    }

    [HarmonyPatch(typeof(Pawn_EquipmentTracker), "Notify_EquipmentAdded")]
    internal static class PawnEquipmentracker_NotifyEquipmentAdded_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref ThingWithComps eq, ref Pawn_EquipmentTracker __instance)
        {
            Pawn pawn = (Pawn) __instance?.ParentHolder;
            Log.Message("T: "+ pawn + eq.def + eq.def.BaseMass + pawn.Faction);
            if (pawn == null || 
                eq.def == null ||
                !RaceUtility.PawnUsesChildren(pawn) || 
                AgeStages.IsOlderThan(pawn, AgeStages.Child) ||
                !pawn.Faction.IsPlayer) 
                return;
            if (BnCSettings.OptionChildrenDropWeapons && (eq.def.BaseMass > ChildrenUtility.GetMaxWeaponMass(pawn)))
            {
                Messages.Message(
                    "MessageWeaponTooLarge".Translate(eq.def.label,
                        pawn.Name.ToStringShort), MessageTypeDefOf.CautionInput);
            }
        }
    }

    // Causes children to drop too-heavy weapons and potentially hurt themselves on firing
    [HarmonyPatch(typeof(Verb_Shoot), "TryCastShot")]
    internal static class VerbShoot_TryCastShot_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Verb_Shoot __instance)
        {
            Pawn pawn = __instance?.CasterPawn;
            
            if (pawn == null || 
                !RaceUtility.PawnUsesChildren(pawn) || 
                AgeStages.IsOlderThan(pawn, AgeStages.Child)) 
                return;

            if (BnCSettings.OptionChildrenDropWeapons)
            {
                ChildrenUtility.ApplyRecoil(__instance);
            }
        }
    }

    // Disable baby/toddler meditation
    [HarmonyPatch(typeof(MeditationUtility), "CanMeditateNow")]
    static class MeditationUtility_CanMeditateNow_Patch
    {
        static void Postfix(ref Pawn pawn, ref bool __result)
        {
            if (pawn == null ||
                !RaceUtility.PawnUsesChildren(pawn) ||
                AgeStages.IsYoungerThan(pawn, AgeStages.Child))
            {
                __result = false;
            }

        }
    }



    // Children are downed easier
    [HarmonyPatch(typeof(Pawn_HealthTracker), "ShouldBeDowned")]
    internal static class Pawn_HealtherTracker_ShouldBeDowned_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref Pawn_HealthTracker __instance, ref bool __result)
        {
            Pawn pawn = (Pawn) AccessTools.Field(typeof(Pawn_HealthTracker), "pawn").GetValue(__instance);
            if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsYoungerThan(pawn, AgeStages.Teenager))
            {
                var painShockThreshold = ChildrenUtility.GetPainShockThreshold(pawn);
                __result =
                    __instance.hediffSet.PainTotal >= painShockThreshold ||
                    !__instance.capacities.CanBeAwake || 
                    !__instance.capacities.CapableOf(PawnCapacityDefOf.Moving);
            }
        }
    }
}
