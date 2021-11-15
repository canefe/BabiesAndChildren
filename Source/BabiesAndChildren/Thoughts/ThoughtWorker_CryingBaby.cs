using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using RimWorld;
using Verse;

namespace BabiesAndChildren
{
    public class ThoughtWorker_CryingBaby : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal (Pawn p)
        {
            // Does not affect babies and toddlers
            if (AgeStages.IsYoungerThan(p, AgeStages.Teenager) || p.health.capacities.GetLevel(PawnCapacityDefOf.Hearing) <= 0.1f)
                return ThoughtState.Inactive;

            // Find all crying babies in the vicinity
            int cryingBabies = 0;
            foreach (Pawn mapPawn in p.MapHeld.mapPawns.AllPawnsSpawned) {
                if (RaceUtility.PawnUsesChildren(mapPawn) &&
                    AgeStages.IsAgeStage(mapPawn, AgeStages.Baby) &&
                    mapPawn.health.hediffSet.HasHediff (HediffDef.Named ("UnhappyBaby")) &&
                    mapPawn.PositionHeld.InHorDistOf(p.PositionHeld, 24) &&
                    mapPawn.PositionHeld.GetRoomOrAdjacent(mapPawn.MapHeld).ContainedAndAdjacentThings.Contains(p)){
                    cryingBabies += 1;
                }
            }
            if (cryingBabies > 0) {
                if (cryingBabies == 1)
                    return ThoughtState.ActiveAtStage (0);
                else if (cryingBabies <= 3)
                    return ThoughtState.ActiveAtStage (1);
                else if (cryingBabies >= 4)
                    return ThoughtState.ActiveAtStage (2);
            }
            return ThoughtState.Inactive;
        }
    }
}