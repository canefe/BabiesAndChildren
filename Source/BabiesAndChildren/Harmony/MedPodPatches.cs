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

            MedPodType = AccessTools.TypeByName("MedPod.Patches.Harmony_FloatMenuMakerMap_OverrideRescueWithMedPodVersion");

            MethodInfo original = AccessTools.Method(MedPodType, "AddRescueToMedPodOption");

            HarmonyMethod prefix = new HarmonyMethod(typeof(MedPodPatches), nameof(AddRescueToMedPodOptionPrefix));
            harmony.Patch(original, prefix: prefix);

            MedPodType = AccessTools.TypeByName("MedPod.WorkGiver_DoctorRescueToMedPod");
            original = AccessTools.Method(MedPodType, "HasJobOnThing");

            HarmonyMethod postfix = new HarmonyMethod(typeof(MedPodPatches), nameof(HasJobOnThingPostfix));
            harmony.Patch(original, postfix: postfix);
        }

        private static bool AddRescueToMedPodOptionPrefix(Pawn pawn, Pawn victim, ref FloatMenuOption __result)
        {
            if (RaceUtility.PawnUsesChildren(victim) && AgeStages.IsAgeStage(victim, AgeStages.Baby))
            {
                __result = FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Rescue".Translate(victim.LabelCap, victim), delegate ()
                {
                    Building_Bed building_Bed;

                    building_Bed = RestUtility.FindBedFor(victim, pawn, false, true, null);

                    if (building_Bed == null)
                    {
                        string t = (!victim.RaceProps.Animal) ? "NoNonPrisonerBed".Translate() : "NoAnimalBed".Translate();
                        Messages.Message("CannotRescue".Translate() + ": " + t, victim, MessageTypeDefOf.RejectInput, false);
                    }
                    Job job = JobMaker.MakeJob(JobDefOf.Rescue, victim, building_Bed);
                    job.count = 1;
                    pawn.jobs.TryTakeOrderedJob(job, new JobTag?(JobTag.Misc), false);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.Rescuing, KnowledgeAmount.Total);
                }, MenuOptionPriority.RescueOrCapture, null, victim, 0f, null, null, true, 0), pawn, victim, "ReservedBy");
                return false;
            }

            return true;
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