using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BabiesAndChildren.api;
using RimWorld;
using Verse;

namespace BabiesAndChildren.Tools
{

	public static class WatchCardUtility
	{

		public static void DrawWatchCard(Rect rect, Pawn pawn)
		{
			bool flag = RaceUtility.PawnUsesChildren(pawn) && (AgeStages.IsAgeStage(pawn, AgeStages.Teenager) || AgeStages.IsAgeStage(pawn, AgeStages.Child));
			if (flag)
			{
				Text.Font = GameFont.Small;
				Listing_Standard listing_Standard = new Listing_Standard();
				listing_Standard.Begin(rect);
				listing_Standard.Gap(30f);
				listing_Standard.Label("TabWatch_desc".Translate());
				listing_Standard.Gap(12f);

				Rect rect3 = listing_Standard.GetRect(25f);
				Widgets.Label(rect3, "TabWatch".Translate() + ": ");
				rect3.xMin = rect3.center.x;
				WatchCardUtility.MentorSelectButton(rect3, pawn, false);
				listing_Standard.Gap(12f);
				if (pawn.TryGetComp<Growing_Comp>().mentor != null)
				{
					Rect rect4 = listing_Standard.GetRect(25f);
					listing_Standard.Gap(12f);
					bool onlyMentor = pawn.TryGetComp<Growing_Comp>().onlyMentor;
					Widgets.CheckboxLabeled(rect4, "TabWatch_onlyMentor".Translate(), ref onlyMentor, false, null, null, false);
					TooltipHandler.TipRegion(rect4, "TabWatch_onlyMentor_desc".Translate());
				if (onlyMentor != pawn.TryGetComp<Growing_Comp>().onlyMentor)
				{
					pawn.TryGetComp<Growing_Comp>().onlyMentor = onlyMentor;
				}
				}
				listing_Standard.Gap(12f);
				listing_Standard.End();
			}
		}

		public static void MentorSelectButton(Rect rect, Pawn pawn, bool paintable)
		{
			Widgets.Dropdown<Pawn, Pawn>(rect, pawn, new Func<Pawn, Pawn>(WatchCardUtility.MasterSelectButton_GetMaster), new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<Pawn>>>(WatchCardUtility.MasterSelectButton_GenerateMenu), WatchCardUtility.MasterString(pawn).Truncate(rect.width, null), null, WatchCardUtility.MasterString(pawn), null, null, paintable);
		}
		private static Pawn MasterSelectButton_GetMaster(Pawn pawn)
		{
			return pawn;
		}
		private static IEnumerable<Widgets.DropdownMenuElement<Pawn>> MasterSelectButton_GenerateMenu(Pawn p)
		{
			yield return new Widgets.DropdownMenuElement<Pawn>
			{
				option = new FloatMenuOption("(" + "NoneLower".Translate() + ")", delegate ()
				{
					var comp = p.TryGetComp<Growing_Comp>();
					comp.mentor = null;
				}, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0),
				payload = null
			};
			using (List<Pawn>.Enumerator enumerator = PawnsFinder.AllMaps_FreeColonistsSpawned.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Pawn col = enumerator.Current;
					string text = RelationsUtility.LabelWithBondInfo(col, p);
					Action action = null;
					action = delegate ()
					{
						var comp = p.TryGetComp<Growing_Comp>();
						comp.mentor = col;
					};
					
					if (p != col && !(RaceUtility.PawnUsesChildren(col) && AgeStages.IsYoungerThan(col, AgeStages.Teenager)))
					{
						yield return new Widgets.DropdownMenuElement<Pawn>
						{
							option = new FloatMenuOption(text, action, MenuOptionPriority.Default, null, null, 0f, null, null, true, 0),
							payload = col
						};
					}
				}
			}
			yield break;
		}
		public static void RemoveWorkType(WorkTypeDef def, Map map)
		{
			try
			{
				new HashSet<Pawn>();
				if (map != null && map.mapPawns != null)
				{
					foreach (Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
					{
						if (pawn != null && pawn.workSettings != null)
						{
							pawn.workSettings.Disable(def);
						}
					}
				}
			}
			catch
			{
			}
		}
		public static void RemoveWorkTypeAdult(WorkTypeDef def, Map map)
		{
			try
			{
				new HashSet<Pawn>();
				if (map != null && map.mapPawns != null)
				{
					foreach (Pawn pawn in map.mapPawns.PawnsInFaction(Faction.OfPlayer))
					{
						if (pawn != null && pawn.workSettings != null)
						{
							if (!RaceUtility.PawnUsesChildren(pawn) || (RaceUtility.PawnUsesChildren(pawn) && AgeStages.IsOlderThan(pawn, AgeStages.Teenager))){
								pawn.workSettings.Disable(def);
							}

						}
					}
				}
			}
			catch
			{
			}
		}

		public static string MasterString(Pawn pawn)
		{
			var comp = pawn.TryGetComp<Growing_Comp>();
			
			if (comp.mentor == null)
			{
				return "(" + "NoneLower".TranslateSimple() + ")";
			}
			return RelationsUtility.LabelWithBondInfo(comp.mentor, pawn);
		}

	}
}