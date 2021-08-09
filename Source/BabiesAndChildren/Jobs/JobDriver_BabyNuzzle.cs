using RimWorld;
using System.Collections.Generic;
using System.Linq;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using Verse;
using Verse.AI;
using LifeStageUtility = BabiesAndChildren.Tools.LifeStageUtility;

namespace BabiesAndChildren
{
    public class ThinkNode_ChancePerHour_BabyNuzzle : ThinkNode_ChancePerHour
    {
        private const float BabyMtbHours = 18f;
        protected override float MtbHours(Pawn pawn)
        {
            if (!RaceUtility.PawnUsesChildren(pawn) || 
                AgeStages.IsOlderThan(pawn, AgeStages.Child) ||
                !BnCSettings.child_cute_act_enabled || 
                pawn.story.traits.HasTrait(TraitDefOf.Psychopath)) 
                return -1f;            
            
            if (AgeStages.IsAgeStage(pawn, AgeStages.Child))
            {
                float minChildAge = LifeStageUtility.GetLifeStageAge(pawn, AgeStages.Child).minAge;
                float minTeenAge = LifeStageUtility.GetLifeStageAge(pawn,AgeStages.Teenager).minAge;
                float curAge = pawn.ageTracker.AgeBiologicalYearsFloat + 0.1f; // prevent 0 + 0.1f
                float f = 1f + (3f * (curAge - minChildAge) / (minTeenAge - minChildAge));
                return BabyMtbHours * f;
            }
            else
            {
                return BabyMtbHours;
            }
        }
    }

    public class JobGiver_BabyNuzzle : ThinkNode_JobGiver
    {
        private const float MaxNuzzleDistance = 30f;
        protected override Job TryGiveJob(Pawn pawn)
        {
            if (!RaceUtility.PawnUsesChildren(pawn) || 
                AgeStages.IsOlderThan(pawn, AgeStages.Child) ||
                !BnCSettings.child_cute_act_enabled || 
                pawn.story.traits.HasTrait(TraitDefOf.Psychopath)) 
                return null;

            if (!(from p in pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction)
                where !p.NonHumanlikeOrWildMan() && 
                      (p != pawn && p.Position.InHorDistOf(pawn.Position, MaxNuzzleDistance)) && 
                      (pawn.GetRoom(RegionType.Set_Passable) == p.GetRoom(RegionType.Set_Passable)) && 
                      !p.Position.IsForbidden(pawn)
                      && p.CanCasuallyInteractNow(false)
                select p).TryRandomElement(out var t))
            {
                return null;
            }
            JobDef jobDef = DefDatabase<JobDef>.GetNamed("BabyNuzzle", true);
            Job job = JobMaker.MakeJob(jobDef, t);
            job.locomotionUrgency = LocomotionUrgency.Walk;
            job.expiryInterval = 3000;
            return job;
        }
    }

    public class JobDriver_BabyNuzzle : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            InteractionDef iDef = DefDatabase<InteractionDef>.GetNamed("BabyNuzzle", true);

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnNotCasualInterruptible(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
            Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch).socialMode = RandomSocialMode.Off;
            Toils_General.WaitWith(TargetIndex.A, 100, false, true).socialMode = RandomSocialMode.Off;
            yield return Toils_General.Do(delegate
            {
                Pawn recipient = (Pawn)this.pawn.CurJob.targetA.Thing;
                this.pawn.interactions.TryInteractWith(recipient, iDef);
            });
            yield break;
        }
        private const int NuzzleDuration = 100;
    }

    public class InteractionWorker_BabyNuzzle : InteractionWorker
    {
        private const float JoyAmount = 500f;

        public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
        {
            this.AddBabyNuzzledThought(initiator, recipient);
            letterText = null;
            letterLabel = null;
            letterDef = null;
            lookTargets = null;
        }

        private void AddBabyNuzzledThought(Pawn initiator, Pawn recipient)
        {


            Thought_Memory recipentThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("BabyNuzzled"));
            Thought_Memory initiatorThought = (Thought_Memory)ThoughtMaker.MakeThought(ThoughtDef.Named("ReceivedPraise"));
            if (recipient.needs.mood != null)
            {
                recipient.needs.mood.thoughts.memories.TryGainMemory(recipentThought, null);
            }
            if (initiator.needs.mood != null)
            {
                initiator.needs.mood.thoughts.memories.TryGainMemory(initiatorThought, null);
                initiator.needs.joy.GainJoy(3.5E-05f * JoyAmount, JoyKindDefOf.Social);
            }
        }
    }
}