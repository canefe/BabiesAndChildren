using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;

namespace BabiesAndChildren
{
    public class WorkGiver_FollowLead : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Pawn Mentor = (Pawn) thing;
            if (Mentor == null || Mentor == pawn)
            {
                return false;
            }

            if (!RaceUtility.PawnUsesChildren(pawn) || AgeStages.IsYoungerThan(pawn, AgeStages.Child) || AgeStages.IsOlderThan(pawn, AgeStages.Teenager))
            {
                return false;
            }

            if (!Mentor.IsColonist || Mentor.mindState.IsIdle || Mentor.CurJob == null || Mentor.CurJob.bill == null)
            {
                return false;
            }

            if (FeedPatientUtility.IsHungry(pawn))
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch, Danger.Deadly, 3))
            {
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = (Pawn) t;
            if (pawn2 == null) return null;
            return new Job(DefDatabase<JobDef>.GetNamed("BnC_Watch"))
            {
                targetA = pawn2,
            };


        }
    }

    public class JobDriver_FollowLead : JobDriver
    {
        protected Pawn Mentor => (Pawn) TargetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnMentalState(TargetIndex.A);
            this.FailOnNotAwake(TargetIndex.A);
            
            this.FailOn(() => !Mentor.IsColonist || Mentor.mindState.IsIdle || Mentor.CurJob == null || Mentor.CurJob.bill == null);

            var rangeCondition = new System.Func<Toil, bool>(RangeCondition);

            yield return Toils_Reserve.Reserve(TargetIndex.A, 3, 0, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return MakeWatchToil(Mentor);
            yield break;
            
        }

        
        protected Toil MakeWatchToil(Pawn friend)
        {
            var toil = new Toil();
            toil.tickAction = delegate
            {
                var actor = toil.actor;
                bool flag6 = (actor.Position - friend.Position).LengthHorizontalSquared >= 6 || !GenSight.LineOfSight(actor.Position, friend.Position, actor.Map, true, null, 0, 0);
                if (flag6)
                {
                    Job newJob = JobMaker.MakeJob(JobDefOf.Goto, friend);
                    actor.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false, false);
                    return;

                }
                else
                {
                    actor.Rotation = friend.Rotation;
                    if (Mentor.CurJob.bill.recipe.workSkill != null)
                        actor.skills.Learn(Mentor.CurJob.bill.recipe.workSkill, 0.5f, false);
                }
            };
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            return toil;
        }

        private bool RangeCondition(Toil toil)
        {
            return toil.actor.Position.DistanceTo(Mentor.Position) > 3f;
        }
    }
}