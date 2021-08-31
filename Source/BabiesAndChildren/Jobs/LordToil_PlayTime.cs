using BabiesAndChildren.Defs;
using System;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace BabiesAndChildren
{

	public class LordToil_PlayTime : LordToil
	{

		public LordToil_PlayTime(Pawn[] pawns)
		{
			this.friends = pawns;
		}


		public override void UpdateAllDuties()
		{
			for (int i = 0; i < this.lord.ownedPawns.Count; i++)
			{
				this.lord.ownedPawns[i].mindState.duty = new PawnDuty(BnCDutyDefOf.PlayTime, this.friends[0].Position, this.friends[1].Position, -1f);
			}
		}


		public Pawn[] friends;


		public Job playTime;


		public int ticksToNextJoy = 0;


		public int tickSinceLastJobGiven = 0;
	}
}