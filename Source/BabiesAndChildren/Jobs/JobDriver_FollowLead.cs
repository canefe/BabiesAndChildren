using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using UnityEngine;

namespace BabiesAndChildren
{
    public class WorkGiver_FollowLead : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;
        Dictionary<JobDef, SkillDef> watchedJobs = new Dictionary<JobDef, SkillDef> { { JobDefOf.Hunt, SkillDefOf.Shooting }, { JobDefOf.Sow, SkillDefOf.Plants }, { JobDefOf.Harvest, SkillDefOf.Plants }, { JobDefOf.Train, SkillDefOf.Animals }, { JobDefOf.Tame, SkillDefOf.Animals }, { JobDefOf.TendPatient, SkillDefOf.Medicine }, { JobDefOf.Research, SkillDefOf.Intellectual } };

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            if (!BnCSettings.watchworktype_enabled)
            {
                return false;
            }
            Pawn Mentor = (Pawn) thing;
            if (Mentor == null || Mentor == pawn || Mentor.NonHumanlikeOrWildMan())
            {
                return false;
            }

            if (!RaceUtility.PawnUsesChildren(pawn) || AgeStages.IsOlderThan(pawn, AgeStages.Teenager) || AgeStages.IsYoungerThan(Mentor, AgeStages.Teenager))
            {
                return false;
            }

            if (!Mentor.IsColonist || Mentor.mindState.IsIdle || Mentor.CurJob == null)
            {
                return false;
            }

            //Bill-based job: check if the kid has skill disabled or not
            if (Mentor.CurJob.bill != null && Mentor.CurJob.bill.recipe.workSkill != null && pawn.skills.GetSkill(Mentor.CurJob.bill.recipe.workSkill).TotallyDisabled)
            {
                return false;
            }

            //Bill-based job: ignore if the mentor has lower skill level.
            if (Mentor.CurJob.bill != null && Mentor.CurJob.bill.recipe.workSkill != null && pawn.skills.GetSkill(Mentor.CurJob.bill.recipe.workSkill).Level > Mentor.skills.GetSkill(Mentor.CurJob.bill.recipe.workSkill).Level)
            {
                return false;
            }


            if (Mentor.CurJob.bill == null && watchedJobs.TryGetValue(Mentor.CurJobDef) == null)
                return false;

            if (Mentor.CurJob.bill == null && watchedJobs.TryGetValue(Mentor.CurJobDef) != null && pawn.skills.GetSkill(watchedJobs.TryGetValue(Mentor.CurJobDef)).TotallyDisabled)
            {
                return false;
            }

            //Check if the mentors skill level lower  than the kid
            if (Mentor.CurJob.bill == null && watchedJobs.TryGetValue(Mentor.CurJobDef) != null && pawn.skills.GetSkill(watchedJobs.TryGetValue(Mentor.CurJobDef)).Level > Mentor.skills.GetSkill(watchedJobs.TryGetValue(Mentor.CurJobDef)).Level)
            {
                return false;
            }

            Pawn PMentor = pawn.TryGetComp<Growing_Comp>().mentor;
            if (PMentor != null && pawn.TryGetComp<Growing_Comp>().onlyMentor && Mentor != PMentor) {
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
            var comp = pawn.TryGetComp<Growing_Comp>();
            if (comp != null && comp.mentor != null)
            {
                if (comp.mentor.CurJob != null && (comp.mentor.CurJob.bill != null ? true : watchedJobs.TryGetValue(comp.mentor.CurJobDef) != null))
                {
                    pawn2 = comp.mentor;
                }


            }
            return new Job(BnCJobDefOf.BnC_Watch)
            {
                targetA = pawn2,
            };


        }
    }

    public class JobDriver_FollowLead : JobDriver
    {
        protected Pawn Mentor => (Pawn) TargetA.Thing;
        //HashSet<JobDef> watchedJobs = new HashSet<JobDef> { JobDefOf.Hunt, JobDefOf.Sow };
        Dictionary<JobDef, SkillDef> watchedJobs = new Dictionary<JobDef, SkillDef> { {JobDefOf.Hunt, SkillDefOf.Shooting }, {JobDefOf.Sow, SkillDefOf.Plants }, { JobDefOf.Harvest, SkillDefOf.Plants }, { JobDefOf.Train, SkillDefOf.Animals }, { JobDefOf.Tame, SkillDefOf.Animals }, { JobDefOf.TendPatient, SkillDefOf.Medicine }, { JobDefOf.Research, SkillDefOf.Intellectual } };
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnMentalState(TargetIndex.A);
            this.FailOnNotAwake(TargetIndex.A);
            this.FailOn(() => !Mentor.IsColonist || Mentor.mindState.IsIdle || Mentor.CurJob == null);
            yield return Toils_Reserve.Reserve(TargetIndex.A, 3, 0, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            yield return MakeWatchToil(Mentor);
            yield break;
            
        }

        
        protected Toil MakeWatchToil(Pawn friend)
        {
            var toil = new Toil();
            SkillDef workSkill = null;
            float mentorTotalTeachPower = 1f;
            toil.initAction = delegate
            {
                if (Mentor.CurJob.bill != null)
                {
                    if (Mentor.CurJob.bill.recipe.workSkill != null)
                    {
                        workSkill = Mentor.CurJob.bill.recipe.workSkill;
                        mentorTotalTeachPower = Mentor.skills.GetSkill(workSkill).Level + ((Mentor.skills.GetSkill(SkillDefOf.Social).Level + Mentor.skills.GetSkill(SkillDefOf.Intellectual).Level) * 0.5f);
                    }
                }else if (watchedJobs.ContainsKey(Mentor.CurJobDef))
                {
                    workSkill = watchedJobs.TryGetValue(Mentor.CurJobDef);
                    mentorTotalTeachPower = Mentor.skills.GetSkill(workSkill).Level + ((Mentor.skills.GetSkill(SkillDefOf.Social).Level + Mentor.skills.GetSkill(SkillDefOf.Intellectual).Level) * 0.5f);
                }
                
                CLog.Warning("Befoire Result teaching; " + mentorTotalTeachPower);
                mentorTotalTeachPower *= Mathf.Max(Mentor.health.capacities.GetLevel(PawnCapacityDefOf.Talking), 0.1f);
                mentorTotalTeachPower *= Mathf.Max(Mentor.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation), 0.1f);
                CLog.Message("Mentor capacity talk; " + Mentor.health.capacities.GetLevel(PawnCapacityDefOf.Talking));
                CLog.Message("Mentor capacity man; " + Mentor.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation));
                CLog.Warning("Result teaching; " + mentorTotalTeachPower);
            };
            toil.tickAction = delegate
            {
                var actor = toil.actor;
                bool flag6 = (actor.Position - friend.Position).LengthHorizontalSquared >= 30 || !GenSight.LineOfSight(actor.Position, friend.Position, actor.Map, true, null, 0, 0);
                if (flag6)
                {
                    Job newJob = JobMaker.MakeJob(JobDefOf.Goto, friend);
                    actor.jobs.StartJob(newJob, JobCondition.InterruptForced, null, false, true, null, null, false, false);
                    return;

                }
                else
                {
                    if ((actor.Position - friend.Position).LengthHorizontalSquared <= 6)
                        actor.Rotation = friend.Rotation;
                    else
                        actor.rotationTracker.FaceTarget(friend);

                    if (workSkill != null)
                    {
                        // todo: make it better
                        float xp = 1f;
                        float mentorSkillModifier = Mentor.skills.GetSkill(workSkill).Level / 100f;
                        xp *= mentorSkillModifier;
                        xp *= BnCSettings.watchexpgainmultiplier;

                        actor.skills.Learn(workSkill, xp, false);
                    }

                }
            };
            toil.AddFinishAction(delegate
            {
                if (workSkill != null)
                {
                    toil.actor.skills.Learn(workSkill, 10f * mentorTotalTeachPower * BnCSettings.watchexpgainmultiplier, false);
                }
            });
            toil.defaultCompleteMode = ToilCompleteMode.Never;
            toil.handlingFacing = true;
            return toil;
        }
    }
}