using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;

namespace BabiesAndChildren
{

	public class WorkGiver_TakeBabyToBedAndFeed : WorkGiver_Scanner
	{
		public override PathEndMode PathEndMode => PathEndMode.Touch;
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup (ThingRequestGroup.Pawn);

		public override bool HasJobOnThing (Pawn pawn, Thing t, bool forced = false)
		{
			Pawn baby = (Pawn) t;
			if (baby == null || baby == pawn) {
				return false;
			}
			//Only perform job if baby is toddler
			if (!AgeStages.IsAgeStage(baby, AgeStages.Toddler) || !baby.IsChildSupported()) {
				return false;
			}
			if (!pawn.CanReserveAndReach (t, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, forced)) {
				return false;
			}
			Building_Bed crib = RestUtility.FindBedFor(baby, pawn, false, false);
			if ( crib == null && !baby.InBed())
			{
				JobFailReason.Is("NoCrib".Translate());
				return false;
			}
			if (!FeedPatientUtility.IsHungry(baby)){
				return false;
			}
			LocalTargetInfo target = t;
			if (!pawn.CanReserve(target, 1, -1, null, forced)){
				return false;
			}

			if (ChildrenUtility.CanBreastfeed(pawn))
			{
				return false;
			}

			if (BnCSettings.breastfeed_only && ChildrenUtility.AnyBreastfeeders(baby))
			{
				if (!baby.health.hediffSet.HasHediff(HediffDefOf.Malnutrition))
					return false;
			}

			if (!FoodUtility.TryFindBestFoodSourceFor(pawn, baby, baby.needs.food.CurCategory == HungerCategory.Starving, out var thing, out var thingDef, false)){
				JobFailReason.Is("NoFood".Translate(), null);
				return false;
			}

			return true;
		}
		
		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			var baby = (Pawn)t;
            Building_Bed crib = RestUtility.FindBedFor(baby, pawn, false, false);

            if (baby.InBed() && (ChildrenUtility.IsBedCrib(baby.CurrentBed()) || !ChildrenUtility.IsBedCrib(crib))) {
                crib = baby.CurrentBed();
            }

            Thing thing;
			ThingDef thingDef;
			Thing foodInInv = FoodUtility.BestFoodInInventory(pawn, baby, FoodPreferability.MealSimple);
			if(foodInInv == null){
				FoodUtility.TryFindBestFoodSourceFor(pawn, baby, baby.needs.food.CurCategory == HungerCategory.Starving, out thing, out thingDef, false);
			}
			else{
				thing = foodInInv;
				thingDef = thing.def;
			}
			if (crib != null && thing != null)
			{
				float nutrition = FoodUtility.GetNutrition(baby, thing, thingDef);
				var feedBaby = new Job(DefDatabase<JobDef>.GetNamed("TakeBabyToBedAndFeed"), thing, baby, crib){
					count = FoodUtility.WillIngestStackCountOf(baby, thingDef, nutrition)
				};
				return feedBaby;
			}
			return null;
		}
	}
	
	public class JobDriver_TakeBabyToBedAndFeed : JobDriver
	{
		private const TargetIndex FoodSourceInd = TargetIndex.A;

		private const TargetIndex DelivereeInd = TargetIndex.B;
		
		private const TargetIndex CribInd = TargetIndex.C;

		private const float FeedDurationMultiplier = 1.5f;

		protected Thing Food => job.targetA.Thing;

		protected Pawn Deliveree => (Pawn)job.targetB.Thing;

		protected Building_Bed Crib => (Building_Bed)job.targetC.Thing;

		public override string GetReport()
		{
			if (job.GetTarget(TargetIndex.A).Thing is Building_NutrientPasteDispenser && Deliveree != null)
			{
				return job.def.reportString.Replace("TargetA", ThingDefOf.MealNutrientPaste.label).Replace("TargetB", Deliveree.LabelShort);
			}
			return base.GetReport();
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			Pawn pawn = this.pawn;
			Job job = this.job;
			if (!pawn.Reserve(new LocalTargetInfo(Deliveree), job, 1, -1, null, errorOnFailed)){
				return false;
			}
            if (!ReservationUtility.HasReserved(Deliveree, Crib)) {
                if(!pawn.Reserve(new LocalTargetInfo(Crib), job, 1, -1, null, errorOnFailed)) {
                    return false;
                }
            }

			if (!(TargetThingA is Building_NutrientPasteDispenser) && (this.pawn.inventory == null || !this.pawn.inventory.Contains(TargetThingA)))
			{
				pawn = this.pawn;
				job = this.job;
				if (!pawn.Reserve(new LocalTargetInfo(Food), job, 1, -1, null, errorOnFailed))
				{
					return false;
				}
			}
			return true;
		}

		[DebuggerHidden]
		protected override IEnumerable<Toil> MakeNewToils()
		{
			
			this.FailOnDestroyedNullOrForbidden(DelivereeInd);
			this.FailOnDespawnedNullOrForbidden(CribInd);
            if (!ReservationUtility.HasReserved(Deliveree, Crib)) {
                yield return Toils_Reserve.Reserve(CribInd);
            }
			this.FailOn(() => Deliveree.needs.food.CurLevelPercentage > Deliveree.needs.food.PercentageThreshHungry + 0.05f);
			
			// Find the food
			if (TargetThingA is Building_NutrientPasteDispenser)
			{
				yield return Toils_Goto.GotoThing(FoodSourceInd, PathEndMode.InteractionCell).FailOnForbidden(FoodSourceInd);
				yield return Toils_Ingest.TakeMealFromDispenser(FoodSourceInd, pawn);
			}
			else if (!(pawn.inventory != null && pawn.inventory.Contains(Food)))
			{
				yield return Toils_Goto.GotoThing(FoodSourceInd, PathEndMode.ClosestTouch).FailOnForbidden(FoodSourceInd);
				yield return Toils_Ingest.PickupIngestible(FoodSourceInd, Deliveree);
			}
			// Put the food in our pocket
			yield return Toils_General.PutCarriedThingInInventory();
			
			// Go to the baby
			yield return Toils_Goto.GotoThing(DelivereeInd, PathEndMode.Touch);
			
			// Make sure crib hasn't been destroyed
			yield return Toils_Haul.StartCarryThing(DelivereeInd);
			yield return Toils_Goto.GotoThing(CribInd, PathEndMode.Touch);
            if (ReservationUtility.HasReserved(pawn, Crib)) {
                yield return Toils_Reserve.Release(CribInd);
            }

			yield return new Toil
			{
				initAction = delegate
				{
					Building_Bed DropBed = Crib;
					IntVec3 position = DropBed.Position;
					Thing thing;
					pawn.carryTracker.TryDropCarriedThing(position, ThingPlaceMode.Direct, out thing, null);
					if (!DropBed.Destroyed && (DropBed.OwnersForReading.Contains(Deliveree) || (DropBed.Medical && DropBed.AnyUnoccupiedSleepingSlot) || Deliveree.ownership == null)) {
						Deliveree.mindState.Notify_TuckedIntoBed();
					}
				},
				defaultCompleteMode = ToilCompleteMode.Instant
			};

			yield return new Toil{
				initAction = delegate{
					PawnUtility.ForceWait(Deliveree, Mathf.RoundToInt((float)Food.def.ingestible.baseIngestTicks * FeedDurationMultiplier), pawn, true);
				}
			};

			//Put food in innerContainer if it isn't already there
			if (pawn.inventory != null && !pawn.inventory.innerContainer.Contains(Food) && pawn.inventory.Contains(Food))
			{
				yield return Toils_Misc.TakeItemFromInventoryToCarrier(pawn, FoodSourceInd);
			}
			yield return Toils_Ingest.ChewIngestible(Deliveree, FeedDurationMultiplier, FoodSourceInd).FailOnCannotTouch(DelivereeInd, PathEndMode.Touch);
			yield return Toils_Ingest.FinalizeIngest(Deliveree, FoodSourceInd);

			Deliveree.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("GotFed"), null);
		}
	}
}