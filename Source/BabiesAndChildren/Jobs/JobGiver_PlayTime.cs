using System;
using RimWorld;
using System.Collections.Generic;
using Verse;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;
using BabiesAndChildren.Tools;
using BabiesAndChildren.api;

namespace BabiesAndChildren
{
	public class JobGiver_PlayTime : JobGiver_GetJoy
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			LordToil_PlayTime lordToil_PlayTime = pawn.GetLord().CurLordToil as LordToil_PlayTime;
			Pawn friend = (pawn == lordToil_PlayTime.friends[0]) ? lordToil_PlayTime.friends[1] : lordToil_PlayTime.friends[0];
			bool flag = friend == null;
			Job result;
			if (flag)
			{
				result = null;
			}
			else
			{
				{
					bool flag3 = lordToil_PlayTime.playTime == null || lordToil_PlayTime.ticksToNextJoy < Find.TickManager.TicksGame;
					if (flag3)
					{
						lordToil_PlayTime.playTime = base.TryGiveJob(pawn);
						lordToil_PlayTime.ticksToNextJoy = Find.TickManager.TicksGame + Rand.RangeInclusive(2500, 7500);
					}
					bool flag4 = lordToil_PlayTime.playTime != null && (friend.needs.food == null || friend.needs.food.CurLevel > 0.33f) && pawn.needs.joy.CurLevel < 0.8f && Math.Abs(lordToil_PlayTime.tickSinceLastJobGiven - Find.TickManager.TicksGame) > 1;
					if (flag4)
					{
						Job job = new Job(lordToil_PlayTime.playTime.def);
						job.targetA = lordToil_PlayTime.playTime.targetA;
						job.targetB = lordToil_PlayTime.playTime.targetB;
						job.targetC = lordToil_PlayTime.playTime.targetC;
						job.targetQueueA = lordToil_PlayTime.playTime.targetQueueA;
						job.targetQueueB = lordToil_PlayTime.playTime.targetQueueB;
						job.count = lordToil_PlayTime.playTime.count;
						job.countQueue = lordToil_PlayTime.playTime.countQueue;
						job.expiryInterval = lordToil_PlayTime.playTime.expiryInterval;
						job.locomotionUrgency = lordToil_PlayTime.playTime.locomotionUrgency;
						bool flag5 = job.TryMakePreToilReservations(pawn, false);
						if (flag5)
						{
							lordToil_PlayTime.tickSinceLastJobGiven = Find.TickManager.TicksGame;
							return job;
						}
						pawn.ClearAllReservations(false);
					}
					bool flag6 = (pawn.Position - friend.Position).LengthHorizontalSquared >= 6 || !GenSight.LineOfSight(pawn.Position, friend.Position, pawn.Map, true, null, 0, 0);
					if (flag6)
					{
						if (friend.CurJob != null && friend.CurJob.def != JobDefOf.Goto)
						{
							result = new Job(JobDefOf.Goto, friend);
						}
						else
						{
							pawn.rotationTracker.FaceCell(friend.Position);
							result = null;
						}
					}
					else
					{
						if (Rand.Value < 0.8f)
						{
							Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && x.InAllowedArea(pawn) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false) && (x - friend.Position).LengthHorizontalSquared < 50 && GenSight.LineOfSight(x, friend.Position, pawn.Map, true, null, 0, 0) && x != friend.Position;
							IntVec3 intVec;
							bool flag8 = CellFinder.TryFindRandomReachableCellNear(pawn.Position, pawn.Map, 12f, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => validator(x), null, out intVec, 999999);
							if (flag8)
							{
								bool flag9 = (pawn.Position - friend.Position).LengthHorizontalSquared >= 5 || (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.Position != friend.Position);
								if (flag9)
								{
									pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
									bool flag10 = !intVec.IsValid || pawn.mindState.nextMoveOrderIsWait;
									if (flag10)
									{
										pawn.rotationTracker.FaceCell(friend.Position);
										return null;
									}
								}
								Job job2 = new Job(BnCJobDefOf.PlayAround, intVec);
								pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job2, intVec);
								result = job2;
							}
							else
							{
								pawn.rotationTracker.FaceCell(friend.Position);
								result = null;
							}
                        }
                        else {
							Predicate<IntVec3> validator = (IntVec3 x) => x.Standable(pawn.Map) && x.InAllowedArea(pawn) && !x.IsForbidden(pawn) && pawn.CanReserveAndReach(x, PathEndMode.OnCell, Danger.None, 1, -1, null, false) && (x - friend.Position).LengthHorizontalSquared < 50 && GenSight.LineOfSight(x, friend.Position, pawn.Map, true, null, 0, 0) && x != friend.Position;
							IntVec3 intVec;
							bool flag8 = CellFinder.TryFindRandomReachableCellNear(pawn.Position, pawn.Map, 12f, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false, false, false), (IntVec3 x) => validator(x), null, out intVec, 999999);
							if (flag8)
							{
								bool flag9 = (pawn.Position - friend.Position).LengthHorizontalSquared >= 5 || (LovePartnerRelationUtility.LovePartnerRelationExists(pawn, friend) && pawn.Position != friend.Position);
								if (flag9)
								{
									pawn.mindState.nextMoveOrderIsWait = !pawn.mindState.nextMoveOrderIsWait;
									bool flag10 = !intVec.IsValid || pawn.mindState.nextMoveOrderIsWait;
									if (flag10)
									{
										pawn.rotationTracker.FaceCell(friend.Position);
										return null;
									}
								}
								Job job2 = new Job(JobDefOf.GotoWander, intVec);
								pawn.Map.pawnDestinationReservationManager.Reserve(pawn, job2, intVec);
								result = job2;
                            }
                            else
                            {
								pawn.rotationTracker.FaceCell(friend.Position);
								result = null;
                            }
						}
					}
				}
			}
			return result;
		}
	}
	public class InteractionWorker_PlayTime : InteractionWorker
	{
		public override float RandomSelectionWeight(Pawn initiator, Pawn recipient)
		{
			
			if (!BnCSettings.playtime_enabled)
            {
				return 0f;
            }
			if (!RaceUtility.PawnUsesChildren(initiator) || !RaceUtility.PawnUsesChildren(recipient))
            {
				return 0f;
            }
			if (initiator.IsSlave || recipient.IsSlave) // since they cant gain joy
            {
				return 0f;
            }
			if (initiator.IsPrisoner || recipient.IsPrisoner) // since they cant gain joy
			{
				return 0f;
			}
			if (!AgeStages.IsYoungerThan(recipient, AgeStages.Teenager) || !AgeStages.IsYoungerThan(initiator, AgeStages.Teenager))
            {
				return 0f;
            }
			if (AgeStages.IsAgeStage(recipient, AgeStages.Baby) || AgeStages.IsAgeStage(recipient, AgeStages.Baby))
			{
				return 0f;
			}
			if (initiator.GetLord() != null || recipient.GetLord() != null)
			{
				return 0f;
			}
			float num3 = 0f;
			bool flag9 = initiator.GetTimeAssignment() == TimeAssignmentDefOf.Anything;
			if (flag9)
			{
				num3 = 1.1f;
			}
			else
			{
				bool flag10 = initiator.GetTimeAssignment() == TimeAssignmentDefOf.Joy;
				if (flag10)
				{
					num3 = 1.4f;
				}
			}
			if (initiator.mindState.IsIdle && recipient.mindState.IsIdle && initiator.GetTimeAssignment() != TimeAssignmentDefOf.Work && recipient.GetTimeAssignment() != TimeAssignmentDefOf.Work)
			{
				num3 = 2f;
			}
			return 0.5f * num3;
		}

		public override void Interacted(Pawn initiator, Pawn recipient, List<RulePackDef> extraSentencePacks, out string letterText, out string letterLabel, out LetterDef letterDef, out LookTargets lookTargets)
		{
			letterText = null;
			letterLabel = null;
			letterDef = null;
			lookTargets = null;
			initiator.jobs.StopAll(false, true);
			recipient.jobs.StopAll(false, true);
			Lord lord = LordMaker.MakeNewLord(initiator.Faction, new LordJob_PlayTime(initiator, recipient), initiator.Map, new Pawn[]
			{
			initiator,
			recipient
			});
		}
	}

}
