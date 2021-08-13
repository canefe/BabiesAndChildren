using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;

namespace BabiesAndChildren
{
    public class WorkGiver_BreastfeedBaby : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override bool HasJobOnThing(Pawn pawn, Thing thing, bool forced = false)
        {
            Pawn pawn2 = (Pawn) thing;
            if (pawn2 == null || pawn2 == pawn)
            {
                return false;
            }

            if (!RaceUtility.PawnUsesChildren(pawn2) || AgeStages.IsOlderThan(pawn2, AgeStages.Toddler))
            {
                return false;
            }

            if (!FeedPatientUtility.IsHungry(pawn2))
            {
                return false;
            }

            if (!pawn2.InBed())
            {
                return false;
            }

            if (!ChildrenUtility.ShouldBeFed(pawn2))
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(thing, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
            {
                return false;
            }

            if (!ChildrenUtility.CanBreastfeed(pawn))
            {
                JobFailReason.Is("ReasonCannotBreastfeed".Translate(pawn.LabelShort));
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = (Pawn) t;
            if (pawn2 == null) return null;
            if (ChildrenUtility.CanBreastfeed(pawn))
            {
                return new Job(DefDatabase<JobDef>.GetNamed("BreastfeedBaby"))
                {
                    targetA = pawn2,
                };
            }

            return null;
        }
    }

    public class JobDriver_BreastfeedBaby : JobDriver
    {
        private const int breastFeedDuration = 600;

        protected Pawn Victim => (Pawn) TargetA.Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(Victim, job, 1, -1, null);
        }


        [DebuggerHidden]
        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            this.FailOnSomeonePhysicallyInteracting(TargetIndex.A);
            this.FailOn(delegate
            {
                if (!ChildrenUtility.CanBreastfeed(pawn) || !pawn.CanReserve(TargetA, 1, -1, null, false))
                    return true;
                else return false;
            });

            yield return Toils_Reserve.Reserve(TargetIndex.A, 1, -1, null);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil prepare = new Toil();
            prepare.initAction = delegate
            {
                if (AgeStages.IsOlderThan(Victim, AgeStages.Baby))
                    PawnUtility.ForceWait(Victim, breastFeedDuration, Victim);
            };
            prepare.defaultCompleteMode = ToilCompleteMode.Delay;
            prepare.defaultDuration = breastFeedDuration;
            yield return prepare;
            yield return new Toil
            {
                initAction = delegate
                {
                    AddEndCondition(() => JobCondition.Succeeded);
                    // Baby is full
                    Victim.needs.food.CurLevelPercentage = 1f;
                    Victim.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("GotFed"), null);
                },

                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }

    public class WorkGiver_FoodFeedBaby : WorkGiver_Scanner
    {
        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;

            if (pawn2 == null ||
                pawn2 == pawn ||
                !RaceUtility.PawnUsesChildren(pawn2) ||
                AgeStages.IsOlderThan(pawn2, AgeStages.Toddler))
            {
                return false;
            }

            if (!FeedPatientUtility.IsHungry(pawn2))
            {
                return false;
            }

            if (!pawn2.InBed())
            {
                return false;
            }

            if (!ChildrenUtility.ShouldBeFed(pawn2))
            {
                return false;
            }

            if (!pawn.CanReserveAndReach(t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced))
            {
                return false;
            }

            if (ChildrenUtility.CanBreastfeed(pawn))
            {
                return false;
            }

            if (!FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2,
                pawn2.needs.food.CurCategory == HungerCategory.Starving, out var thing, out var thingDef, false))
            {
                JobFailReason.Is("NoFood".Translate(), null);
                return false;
            }

            return true;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            var pawn2 = (Pawn) t;

            if (!pawn2.InBed()) return null;

            Thing thing;
            ThingDef thingDef;
            Thing foodInInv = FoodUtility.BestFoodInInventory(pawn, pawn2, FoodPreferability.MealSimple);
            if (foodInInv == null)
            {
                FoodUtility.TryFindBestFoodSourceFor(pawn, pawn2,
                    pawn2.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false);
            }
            else
            {
                thing = foodInInv;
                thingDef = thing.def;
            }

            if (thing == null) return null;

            float nutrition = FoodUtility.GetNutrition(thing, thingDef);
            var feedBaby = new Job(DefDatabase<JobDef>.GetNamed("FoodFeedBaby"), thing, pawn2)
            {
                count = FoodUtility.WillIngestStackCountOf(pawn2, thingDef, nutrition)
            };
            pawn2.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("GotFed"), null);
            return feedBaby;
        }
    }
}