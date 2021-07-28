using RimWorld;
using System.Linq;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using Verse;

namespace BabiesAndChildren
{
    public static class DebugActions
    {
        public const string debugCategory = "Babies and Children";
        
        [DebugAction(debugCategory, "Reinitialize Children", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void Reinitialize()
        {
            ChildrenBase.ReinitializeChildren(Current.Game.CurrentMap);
        }

        [DebugAction(debugCategory, "Change Child Backstory", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ChangeChildBackstory()
        {
            foreach (Thing thing in UI.MouseCell().GetThingList(Find.CurrentMap).ToList<Thing>())
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.GetAgeStage(pawn) < AgeStages.Teenager)
                    {
                        StoryUtility.ChangeChildhood(pawn);
                    }
                }
            }
        }

        [DebugAction(debugCategory, "Change Baby Backstory", actionType = DebugActionType.ToolMap, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static void ChangeBabyBackstory()
        {
            foreach (Thing thing in UI.MouseCell().GetThingList(Find.CurrentMap).ToList<Thing>())
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    if (RaceUtility.PawnUsesChildren(pawn) && AgeStages.GetAgeStage(pawn) < AgeStages.Child)
                    {
                        if (pawn.story.childhood == BackstoryDatabase.allBackstories["CustomBackstory_NA_Childhood_Disabled"])
                        {
                            pawn.story.childhood = BackstoryDatabase.allBackstories["CustomBackstory_Rimchild"];
                            pawn.Notify_DisabledWorkTypesChanged();
                            pawn.skills.Notify_SkillDisablesChanged();
                            MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
                        }
                        else Messages.Message("Choose a child who has a backstory 'Baby'", MessageTypeDefOf.NeutralEvent);
                    }
                    else Messages.Message("Choose a baby ", MessageTypeDefOf.NeutralEvent);
                }
            }
        }

    }
}