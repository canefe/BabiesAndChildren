using System.Collections.Generic;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using System.Xml;

namespace BabiesAndChildren.Harmony
{

    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    static class FloatMenuMaker_Patch
    {
        public static TargetingParameters TargetParameters
        {
            get
            {
                if (targetParameters == null)
                {
                    targetParameters = new TargetingParameters()
                    {
                        canTargetHumans = true,
                        canTargetAnimals = false,
                        canTargetItems = false,
                        mapObjectTargetsMustBeAutoAttackable = false,
                    };
                }
                return targetParameters;
            }
        }

        private static TargetingParameters targetParameters = null;

        [HarmonyPostfix]
        public static void MapFloatMenuOption(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {


            var validtargets = GenUI.TargetsAt(clickPos, TargetParameters);
            foreach (LocalTargetInfo target in validtargets)
            {

                Pawn pawn2 = target.Thing as Pawn;
                bool flag = pawn2 != null && !pawn.Drafted && pawn.IsChildSupported() && pawn2.IsChildSupported() && (pawn.GetLord() == null);
                bool flag2 = (pawn == pawn2) || (pawn2 is null);
                if (flag2)
                    return;

                if (pawn2.jobs == null)
                    return;

                if (pawn2.RaceProps == null)
                    return;

                if (flag)
                {
                    BnC_FloatMenuUtility.AddPlayOptions(pawn2, pawn, opts);
                }
            }
        }
    }

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
                if (thing.def.thingSetMakerTags == null || !thing.def.thingSetMakerTags.Contains("BabyGear"))
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
            if (!__result && ChildrenUtility.ShouldBeFed(pawn) && pawn.InBed())
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
            if (!__result && p.IsPrisonerOfColony && ChildrenUtility.ShouldBeFed(p) && p.InBed())
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
            if (!__result && ChildrenUtility.ShouldBeFed(p) && p.InBed())
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
            if (pawn.IsChildSupported() && !AgeStages.IsAgeStage(pawn, AgeStages.Adult))
            {
                __result = __result && !Thoughts.IsBlacklisted(def, AgeStages.GetAgeStage(pawn));
            }
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
            if (pawn == null || pawn.Faction == null ||
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
                (RaceUtility.PawnUsesChildren(pawn) &&
                AgeStages.IsYoungerThan(pawn, AgeStages.Child)))
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
    // Cribs won't make a bedroom => barracks
    [HarmonyPatch(typeof(RoomRoleWorker_Barracks), "GetScore")]
    public static class RoomRoleWorker_Barracks_GetScore_Patch
    {
        public static void Postfix(RoomRoleWorker_Bedroom __instance, Room room, ref float __result)
        {
            int num = 0;
            int num2 = 0;
            List<Thing> containedAndAdjacentThings = room.ContainedAndAdjacentThings;
            List<Pawn> list = null;
            for (int i = 0; i < containedAndAdjacentThings.Count; i++)
            {
                Building_Bed building_Bed;
                if ((building_Bed = (containedAndAdjacentThings[i] as Building_Bed)) != null && building_Bed.def.building.bed_humanlike && building_Bed.def.defName != "Crib")
                {
                    if (building_Bed.ForPrisoners)
                    {
                        __result = 0f;
                    }
                    num++;
                    if (!building_Bed.Medical)
                    {
                        List<Pawn> assignedPawnsForReading = building_Bed.CompAssignableToPawn.AssignedPawnsForReading;
                        if (!assignedPawnsForReading.NullOrEmpty<Pawn>())
                        {
                            if (list == null)
                            {
                                list = assignedPawnsForReading[0].GetLoveCluster();
                            }
                            using (List<Pawn>.Enumerator enumerator = assignedPawnsForReading.GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    Pawn item = enumerator.Current;
                                    if (!list.Contains(item))
                                    {
                                        num2++;
                                        break;
                                    }
                                }
                                goto IL_C5;
                            }
                        }
                        num2++;
                    }
                }
            IL_C5:;
            }
            if (num <= 1)
            {
                __result = 0f;
            }
            __result = (float)num2 * 100100f;
        }

    }

    [HarmonyPatch(typeof(WorkGiver_FeedPatient), "HasJobOnThing")]
    internal static class WorkGiver_FeedPatient_Patch
    {
        [HarmonyPostfix]
        static void Postfix(ref bool __result, Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn2 == null || pawn2 == pawn)
            {
                __result = false;
            }

            if (RaceUtility.PawnUsesChildren(pawn2) && AgeStages.IsYoungerThan(pawn2, AgeStages.Child))
            {
                __result = false;
            }
        }
    }

    //TODO: FIND A BETTER WAY TO ENABLE TODDLERS TO USE PLAY INTERACTION
    // THIS IS LAGGY
    
    [HarmonyPatch(typeof(InteractionUtility), "CanInitiateInteraction")]
    internal static class Pawn_InteractionUtility_CanInitiateInteraction_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, InteractionDef interactionDef, ref bool __result)
        {
            if (pawn.IsChildSupported() && AgeStages.IsAgeStage(pawn, AgeStages.Toddler))
            {
                if (interactionDef != null && interactionDef.defName == "PlayTime")
                {
                    __result = true;
                }
                else
                {
                    __result = true;
                }
                if (interactionDef != null && interactionDef.defName != "PlayTime")
                {
                    __result = false;
                }
            }
        }
    }


    [HarmonyPatch(typeof(PawnUtility), "IsInteractionBlocked")]
    internal static class Pawn_PawnUtility_IsInteractionBlocked_Patch
    {
        [HarmonyPostfix]
        static void Postfix(Pawn pawn, InteractionDef interaction, ref bool __result)
        {
            if (pawn.IsChildSupported() && AgeStages.IsAgeStage(pawn, AgeStages.Toddler) && interaction == DefDatabase<InteractionDef>.GetNamed("PlayTime"))
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(MentalBreakWorker), nameof(MentalBreakWorker.BreakCanOccur))]
    public class Patch_MentalBreakWorker_BreakCanOccur
    {
        private static void Postfix(Pawn pawn, ref bool __result, ref MentalBreakWorker __instance)
        {
            if (pawn.IsChildSupported() && AgeStages.IsYoungerThan(pawn, AgeStages.Child))
                __result = __result && __instance.def == DefDatabase<MentalBreakDef>.GetNamed("Tantrum");
        }
    }




}
