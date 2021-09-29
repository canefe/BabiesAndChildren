using System;
using RimWorld;
using Verse;
using Verse.AI.Group;

namespace BabiesAndChildren
{
	internal class LordJob_PlayTime : LordJob
	{
		public LordJob_PlayTime()
		{
		}

		public LordJob_PlayTime(Pawn initiator, Pawn recipient)
		{
			this.initiator = initiator;
			this.recipient = recipient;
		}

		public override StateGraph CreateGraph()
		{
			StateGraph stateGraph = new StateGraph();
			LordToil_PlayTime lordToil_PlayTime = new LordToil_PlayTime(new Pawn[]
			{
				this.initiator,
				this.recipient
			});
			stateGraph.AddToil(lordToil_PlayTime);
			LordToil_End lordToil_End = new LordToil_End();
			stateGraph.AddToil(lordToil_End);
			Transition transition = new Transition(lordToil_PlayTime, lordToil_End, false, true);
			transition.AddTrigger(new Trigger_TickCondition(() => this.ShouldBeCalledOff(), 1));
			transition.AddTrigger(new Trigger_TickCondition(() => this.initiator.health.summaryHealth.SummaryHealthPercent < 1f || this.recipient.health.summaryHealth.SummaryHealthPercent < 1f, 1));
			transition.AddTrigger(new Trigger_TickCondition(() => this.initiator.Drafted || this.recipient.Drafted, 1));
			transition.AddTrigger(new Trigger_TickCondition(() => this.initiator.Map == null || this.recipient.Map == null, 1));
			transition.AddTrigger(new Trigger_PawnLost(PawnLostCondition.Undefined, null));
			stateGraph.AddTransition(transition, false);
			this.timeoutTrigger = new Trigger_TicksPassed(Rand.RangeInclusive(2500, 5000));
			Transition transition2 = new Transition(lordToil_PlayTime, lordToil_End, false, true);
			transition2.AddTrigger(this.timeoutTrigger);
			transition2.AddPreAction(new TransitionAction_Custom(delegate ()
			{
				this.Finished();
			}));
			stateGraph.AddTransition(transition2, false);
			return stateGraph;
		}

		public void Finished()
		{
			this.initiator.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.ChildGames, this.recipient, null);
			this.recipient.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.ChildGames, this.initiator, null);
		}

		public override void ExposeData()
		{
			Scribe_References.Look<Pawn>(ref this.initiator, "initiator", false);
			Scribe_References.Look<Pawn>(ref this.recipient, "recipient", false);
		}

		public override string GetReport(Pawn pawn)
		{
			return "LordJobPlayAround".Translate();
		}

		private bool ShouldBeCalledOff()
		{
			return !GatheringsUtility.AcceptableGameConditionsToContinueGathering(base.Map) || this.initiator.GetTimeAssignment() == TimeAssignmentDefOf.Work || this.recipient.GetTimeAssignment() == TimeAssignmentDefOf.Work || (this.initiator.needs.rest != null && this.initiator.needs.rest.CurLevel < 0.3f) || (this.recipient.needs.rest != null && this.recipient.needs.rest.CurLevel < 0.3f) || (this.initiator.needs.rest == null && this.recipient.needs.rest == null && this.initiator.needs.joy.CurLevel > 0.9f && this.recipient.needs.joy.CurLevel > 0.9f);
		}

		private Trigger_TicksPassed timeoutTrigger;

		public Pawn initiator;

		public Pawn recipient;
	}
}
