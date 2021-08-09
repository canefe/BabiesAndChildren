using RimWorld;
using System;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using Verse;

namespace BabiesAndChildren
{
    public class ThoughtWorker_CarryingToy : ThoughtWorker
    {
        private bool CheckGreedyJealousChild(Pawn pawn)
        {
            return pawn.story.traits.HasTrait(TraitDefOf.Greedy) || pawn.story.traits.HasTrait(TraitDefOf.Jealous);
        }

        protected override ThoughtState CurrentStateInternal(Pawn pawn)
        {
            if (!RaceUtility.PawnUsesChildren(pawn) || 
            !AgeStages.IsAgeStage(pawn, AgeStages.Child) ||
            pawn.story.traits.HasTrait(TraitDefOf.Psychopath) || 
            pawn.WorkTagIsDisabled(WorkTags.Violent) ||
            !pawn.Faction.IsPlayer)
                return false;
            ThingWithComps toy = pawn.equipment.Primary;
            if (toy != null && ChildrenUtility.SetMakerTagCheck(toy,"Toy"))
            {
                CompQuality toyQuality = toy.TryGetComp<CompQuality>();
                if (toyQuality != null)
                {
                    switch (toyQuality.Quality)
                    {
                        case QualityCategory.Awful:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(16);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(23);
                            return ThoughtState.ActiveAtStage(9);

                        case QualityCategory.Poor:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(17);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(24);
                            return ThoughtState.ActiveAtStage(10);

                        case QualityCategory.Normal:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(18);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(25);
                            return ThoughtState.ActiveAtStage(11);

                        case QualityCategory.Good:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(19);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(26);
                            return ThoughtState.ActiveAtStage(12);

                        case QualityCategory.Excellent:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(20);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(27);
                            return ThoughtState.ActiveAtStage(13);

                        case QualityCategory.Masterwork:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(21);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(28);
                            return ThoughtState.ActiveAtStage(14);

                        case QualityCategory.Legendary:
                            if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(22);
                            else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(29);
                            return ThoughtState.ActiveAtStage(15);

                        default:
                            throw new ArgumentException();
                    }
                }
            }
            else
            {
                ExpectationDef expectation = ExpectationsUtility.CurrentExpectationFor(pawn);
                if (expectation == ExpectationDefOf.VeryLow || expectation == ExpectationDefOf.ExtremelyLow)
                {
                    if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(3);
                    else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(6);
                    return ThoughtState.ActiveAtStage(0);
                }
                if (expectation == ExpectationDefOf.Low || expectation == ExpectationDefOf.Moderate)
                {
                    if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(4);
                    else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(7);
                    return ThoughtState.ActiveAtStage(1);
                }
                if (expectation == ExpectationDefOf.High || expectation == ExpectationDefOf.SkyHigh)
                {
                    if (CheckGreedyJealousChild(pawn)) return ThoughtState.ActiveAtStage(5);
                    else if (pawn.story.traits.HasTrait(TraitDefOf.Ascetic)) return ThoughtState.ActiveAtStage(8);
                    return ThoughtState.ActiveAtStage(2);
                }
            }
            return false;
        }
    }
}