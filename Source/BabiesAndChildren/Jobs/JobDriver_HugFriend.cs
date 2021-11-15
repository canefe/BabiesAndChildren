using BabiesAndChildren.api;
using RimWorld;
using Verse;

namespace BabiesAndChildren
{
	public class InteractionWorker_HugFriend : InteractionWorker
	{
		public override float RandomSelectionWeight (Pawn initiator, Pawn recipient)
		{
			if (initiator.relations.OpinionOf(recipient) >= 50 && 
			    initiator.needs.mood.CurLevel >= 0.9f && 
			    !AgeStages.IsOlderThan(initiator, AgeStages.Child) && 
			    !AgeStages.IsAgeStage(initiator, AgeStages.Baby))
			{
				return 0.2f;
			}

			return 0;
		}
	}
}