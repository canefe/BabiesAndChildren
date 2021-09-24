using System;
using RimWorld;
using UnityEngine;
using BabiesAndChildren.Tools;
using BabiesAndChildren.api;
using Verse;

namespace BabiesAndChildren
{

	public class ITab_Pawn_Watch : ITab
	{

		public ITab_Pawn_Watch()
		{
			this.size = new Vector2(300f, 260f);
			this.labelKey = "TabWatch";
			this.tutorTag = "Watch";
		}


		protected override void FillTab()
		{
			Rect rect = new Rect(0f, 0f, this.size.x, this.size.y).ContractedBy(17f);
			rect.yMin += 10f;
			WatchCardUtility.DrawWatchCard(rect, this.PawnToShowInfoAbout);
		}

		public override bool IsVisible
		{
			get
			{
				return BnCSettings.watchworktype_enabled && RaceUtility.PawnUsesChildren(this.PawnToShowInfoAbout) && (AgeStages.IsAgeStage(this.PawnToShowInfoAbout, AgeStages.Teenager) || AgeStages.IsAgeStage(this.PawnToShowInfoAbout, AgeStages.Child)) && this.PawnToShowInfoAbout.workSettings.WorkIsActive(BnCWorkTypeDefOf.BnC_Watch);
			}
		}

		private Pawn PawnToShowInfoAbout
		{
			get
			{
				Pawn pawn = null;
				bool flag = (base.SelPawn != null) && (base.SelPawn.IsSlaveOfColony || base.SelPawn.IsColonist) && !base.SelPawn.health.Dead;
				if (flag)
				{
					pawn = base.SelPawn;
				}
				bool flag3 = pawn == null;
				Pawn result;
				if (flag3)
				{
					result = null;
				}
				else
				{
					result = pawn;
				}
				return result;
			}
		}
	}
}