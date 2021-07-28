using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using Verse;
using Verse.AI;
using RimWorld;

namespace BabiesAndChildren
{

    public class WorkGiver_TakeBabyToCrib : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup (ThingRequestGroup.Pawn);

        public override bool HasJobOnThing (Pawn pawn, Thing t, bool forced = false)
        {
            if (t == null || t == pawn || !(t is Pawn))
                return false;

            Pawn baby = (Pawn) t;
            if (AgeStages.IsOlderThan(baby, AgeStages.Baby) || !RaceUtility.PawnUsesChildren(baby) ) {
                return false;
            }
            if (!pawn.CanReserveAndReach (t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced)) {
                return false;
            }
           
            Building_Bed crib = RestUtility.FindBedFor(baby, pawn, false, false);
            if (baby.InBed() && (ChildrenUtility.IsBedCrib(baby.CurrentBed()) || !ChildrenUtility.IsBedCrib(crib)))
            {
                return false;
            }
            if (crib == null) {
                JobFailReason.Is("NoCrib".Translate());
                return false;
            }
            //If not in a crib, but can't find a crib to go to

            // Is it time for the baby to go to bed?
            var timetable = baby.timetable;
            if (timetable == null || baby.Map == null || !timetable.GetAssignment(GenLocalDate.HourInteger(baby.Map)).allowRest)
                return false;
            
            return true;
        }
        
        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var baby = (Pawn)t;
            Building_Bed crib = RestUtility.FindBedFor(baby, pawn, false, false);
            if (baby != null && crib != null){
                Job carryBaby = new Job(DefDatabase<JobDef>.GetNamed("TakeBabyToCrib"), baby, crib){ count = 1 };
                return carryBaby;
            }
            return null;
        }
    }
}