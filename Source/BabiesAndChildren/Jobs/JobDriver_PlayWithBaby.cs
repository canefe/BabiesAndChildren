using RimWorld;
using System.Collections.Generic;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using Verse;
using Verse.AI;

namespace BabiesAndChildren
{
    public class WorkGiver_PlayWithBaby : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.InteractionCell;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override IEnumerable<Thing> PotentialWorkThingsGlobal(Pawn pawn)
        {
            return pawn.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
        }


        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            if (!InteractionUtility.CanInitiateInteraction(pawn, null))
            {
                return true;
            }
            List<Pawn> list = pawn.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
            foreach (var t in list)
            {
                if (t.InBed())
                {
                    return false;
                }
            }
            return true;
        }


        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn baby = t as Pawn;
            return baby != null && BabyVisitUtility.CanVisit(pawn, baby, JoyCategory.Low);
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn baby = (Pawn)t;
            Job job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("PlayWithBaby"), baby, SickPawnVisitUtility.FindChair(pawn, baby));
            job.ignoreJoyTimeAssignment = true;
            return job;
        }
    }


    public class JobDriver_PlayWithBaby : JobDriver
    {
        private const TargetIndex PatientInd = TargetIndex.A;
        private const TargetIndex ChairInd = TargetIndex.B;

        private Pawn Baby => (Pawn)this.job.GetTarget(TargetIndex.A).Thing;

        private Thing Chair => this.job.GetTarget(TargetIndex.B).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.Baby, this.job, 1, -1, null, errorOnFailed) && (this.Chair == null || this.pawn.Reserve(this.Chair, this.job, 1, -1, null, errorOnFailed));
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() =>  !this.Baby.Awake());
            if (this.Chair != null)
            {
                this.FailOnDespawnedNullOrForbidden(TargetIndex.B);
            }

            if (this.Chair != null)
            {

                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.OnCell);
            }
            else
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            }
            yield return Toils_Interpersonal.WaitToBeAbleToInteract(this.pawn);
            yield return new Toil
            {
                tickAction = delegate ()
                {
                    this.Baby.needs.joy.GainJoy(this.job.def.joyGainRate * 0.000644f, this.job.def.joyKind);
                    if (this.pawn.IsHashIntervalTick(320))
                    {
                        InteractionDef intDef = (Rand.Value < 0.8f) ? DefDatabase<InteractionDef>.GetNamed("BabyGames", true) : InteractionDefOf.Chitchat;
                        this.pawn.interactions.TryInteractWith(this.Baby, intDef);
                    }
                    this.pawn.rotationTracker.FaceCell(this.Baby.Position);
                    this.pawn.GainComfortFromCellIfPossible(false);
                    // Ideology slaves does not have joy stuff thats why this is needed.
                    switch (pawn.IsSlave)
                    {
                        case false:
                            JoyUtility.JoyTickCheckEnd(this.pawn, JoyTickFullJoyAction.None, 1f, null);
                            if (this.pawn.needs.joy.CurLevelPercentage > 0.9999f && this.Baby.needs.joy.CurLevelPercentage > 0.9999f)
                            {
                                this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                            }
                            break;


                        case true:
                            if (this.Baby.needs.joy.CurLevelPercentage > 0.9999f) 
                            {
                                this.pawn.jobs.EndCurrentJob(JobCondition.Succeeded, true, true);
                            }
                            break;

                    }

                },
                handlingFacing = true,
                socialMode = RandomSocialMode.Off,
                defaultCompleteMode = ToilCompleteMode.Delay,
                defaultDuration = this.job.def.joyDuration
            };
            yield break;
        }
    }

    public static class BabyVisitUtility
    {
        public static bool CanVisit(Pawn pawn, Pawn baby, JoyCategory maxPatientJoy)
        {
            return baby.IsColonist &&
                !baby.Dead &&
                pawn != baby &&
                //baby.InBed() &&
                baby.Awake() &&
                RaceUtility.PawnUsesChildren(baby) &&
                AgeStages.IsYoungerThan(baby, AgeStages.Child) &&
                !baby.IsForbidden(pawn) &&
                baby.needs.joy != null &&
                baby.needs.joy.CurCategory <= maxPatientJoy &&
                InteractionUtility.CanReceiveInteraction(baby, null) &&
                !baby.needs.food.Starving &&
                //baby.needs.rest.CurLevel > 0.33f &&
                pawn.CanReserveAndReach(baby, PathEndMode.InteractionCell, Danger.None, 1, -1, null, false);
        }
    }

    public class InteractionWorker_BabyGames : InteractionWorker
    {
        public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
        {
            //Toddlers can play babygames with each other casually, otherwise interaction can only be done with baby

            if (AgeStages.IsAgeStage(initiator, AgeStages.Toddler) && AgeStages.IsAgeStage(recipient, AgeStages.Toddler))
                return 1f;
            if (ModTools.IsRobot(recipient) || AgeStages.IsOlderThan(recipient, AgeStages.Toddler))
                return 0;
            return 1f;
        }
    }
}