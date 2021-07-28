using RimWorld;
using Verse;
using System;
using System.Text;
using System.Collections.Generic;

namespace RimWorldChildren
{
	public class Hediff_Baby : HediffWithComps
	{
        //
        // Fields
        //
        // private const int TicksPerYear = 3600000;
        public bool hediff_isOn = true;
        public int accelerated_factor;
        //private long babystatetick = 0;

        // Keeps track of what stage the pawn has grown to
        private int grown_to = 0;

        //
        // Static Fields
        //

        //
        // Methods
        //
        
        public int HediffStage(){
			return grown_to;
		}

		public override string DebugString ()
		{
			StringBuilder stringBuilder = new StringBuilder ();
			stringBuilder.Append (base.DebugString ());
			return stringBuilder.ToString ();
		}	

		public override void PostMake ()
		{
			Severity = Math.Max(0, Severity);
			if (grown_to == 0 && !pawn.health.hediffSet.HasHediff (HediffDef.Named ("NoManipulationFlag"))) {
				pawn.health.AddHediff(HediffDef.Named("NoManipulationFlag"), null, null);
			}
			base.PostMake ();
		}

		public override void ExposeData ()
		{
			//Scribe_Values.LookValue<int> (ref grown_to, "grown_to", 0);
			Scribe_Values.Look<int> (ref grown_to, "grown_to", 0);

			base.ExposeData ();
		}
		
		internal void GrowUpTo (int stage){
			GrowUpTo (stage, true);
		}
		internal void GrowUpTo (int stage, bool generated)
		{
			grown_to = stage;
            accelerated_factor = ChildrenUtility.Setting_Accelerated_Factor(grown_to);

            // Update the Colonist Bar
            PortraitsCache.SetDirty (pawn);
			LongEventHandler.ExecuteWhenFinished (delegate {
				pawn.Drawer.renderer.graphics.ResolveAllGraphics ();
			});
                        
            // At the toddler stage. Now we can move and talk.
            if (stage == 1)
            {
                Severity = Math.Max(0.5f, Severity);
                //pawn.needs.food.
            }
            // Re-enable skills that were locked out from toddlers
            if (stage == 2) {
				if (!generated && ChildrenUtility.IsHumanlikeChild(pawn))
                { pawn.story.childhood = BackstoryDatabase.allBackstories["CustomBackstory_Rimchild"]; }
                if (!generated && !ChildrenUtility.IsHumanlikeChild(pawn))
                { ChildrenUtility.GiveBackstory(ref pawn); }

                // Remove the hidden hediff stopping pawns from manipulating
                if (pawn.health.hediffSet.HasHediff (HediffDef.Named ("NoManipulationFlag"))) {
					pawn.health.hediffSet.hediffs.Remove (pawn.health.hediffSet.GetFirstHediffOfDef (HediffDef.Named ("NoManipulationFlag")));
				}
				Severity = Math.Max(0.75f, Severity);
			}
			// The child has grown to a teenager so we no longer need this effect
			if (stage == 3) {
				if (!generated && pawn.story.childhood.title == "Child") {
					pawn.story.childhood = BackstoryDatabase.allBackstories ["CustomBackstory_Rimchild"];
				}

				// Gain traits from life experiences
				if (pawn.story.traits.allTraits.Count < 3) {
					List<Trait> life_traitpool = new List<Trait>();
					// Try get cannibalism
					if (pawn.needs.mood.thoughts.memories.Memories.Find (x => x.def == ThoughtDefOf.AteHumanlikeMeatAsIngredient) != null) {
						life_traitpool.Add (new Trait(TraitDefOf.Cannibal, 0, false));
					}
					// Try to get bloodlust
					if (pawn.records.GetValue (RecordDefOf.KillsHumanlikes) > 0 || pawn.records.GetValue(RecordDefOf.AnimalsSlaughtered) >= 2) {
						life_traitpool.Add (new Trait(TraitDefOf.Bloodlust, 0, false));
					}
					// Try to get shooting accuracy
					if (pawn.records.GetValue (RecordDefOf.ShotsFired) > 100) {
						life_traitpool.Add (new Trait (TraitDef.Named ("ShootingAccuracy"), 1, false));
					} else if (pawn.records.GetValue (RecordDefOf.ShotsFired) > 100 && (int)pawn.records.GetValue(RecordDefOf.PawnsDowned) == 0) {
						life_traitpool.Add (new Trait (TraitDef.Named ("ShootingAccuracy"), -1, false));
					}
					// Try to get brawler
					else if (pawn.records.GetValue (RecordDefOf.ShotsFired) < 15 && pawn.records.GetValue (RecordDefOf.PawnsDowned) > 1) {
						life_traitpool.Add (new Trait (TraitDefOf.Brawler, 0, false));
					}
					// Try to get Dislikes Men/Women
					int male_rivals = 0;
					int female_rivals = 0;
					foreach (Pawn colinist in Find.AnyPlayerHomeMap.mapPawns.AllPawnsSpawned) {
						if (pawn.relations.OpinionOf (colinist) <= -20) {
							if (colinist.gender == Gender.Male)
								male_rivals++;
							else
								female_rivals++;
						}
					}
					// Find which gender we hate
					if (male_rivals > 3 || female_rivals > 3) {
						if (male_rivals > female_rivals)
							life_traitpool.Add (new Trait (TraitDefOf.DislikesMen, 0, false));
						else if (female_rivals > male_rivals)
							life_traitpool.Add (new Trait (TraitDefOf.DislikesWomen, 0, false));
					}
					// Pyromaniac never put out any fires. Seems kinda stupid
					/*if ((int)pawn.records.GetValue (RecordDefOf.FiresExtinguished) == 0) {
						life_traitpool.Add (new Trait (TraitDefOf.Pyromaniac, 0, false));
					}*/
					// Neurotic
					if (pawn.records.GetValue (RecordDefOf.TimesInMentalState) > 6) {
						life_traitpool.Add (new Trait (TraitDef.Named("Neurotic"), 2, false));
					} else if (pawn.records.GetValue (RecordDefOf.TimesInMentalState) > 3) {
						life_traitpool.Add (new Trait (TraitDef.Named("Neurotic"), 1, false));
					}

					// Girls can turn gay during puberty
					if (pawn.gender == Gender.Female && Rand.Value <= 0.08f && pawn.story.traits.allTraits.Count <= 3) {
						pawn.story.traits.GainTrait (new Trait (TraitDefOf.Gay, 0, true));
					}

					// Now let's try to add some life experience traits
					if (life_traitpool.Count > 0) {
						int i = 3;
						while (pawn.story.traits.allTraits.Count < 3 && i > 0) {
							Trait newtrait = life_traitpool.RandomElement();
							if (pawn.story.traits.HasTrait (newtrait.def) == false)
								pawn.story.traits.GainTrait (newtrait);
							i--;
						}
					}
				}

				pawn.health.RemoveHediff (this);
			}
		}

		public void TickRare (){

            if (pawn.ageTracker.CurLifeStageIndex > grown_to)
            {
                GrowUpTo(grown_to + 1, false);
            }            

            // Accelerated Growth
            if (BnC_Settings.option_accelerated_growth && (pawn.ageTracker.AgeBiologicalYearsFloat < (float)BnC_Settings.option_accelerated_growth_end_age))
            {
                pawn.ageTracker.AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks + ((long)(accelerated_factor)*60);
                //pawn.ageTracker.AgeBiologicalTicks = pawn.ageTracker.AgeBiologicalTicks + ((long)(accelerated_factor) * 100000);
                //Log.Message("" + pawn.LabelIndefinite() + " *** +180 Bio tick : " + pawn.ageTracker.AgeBiologicalTicks);
            }
            else
            {   hediff_isOn = false;   }            

            // Update the graphics set
            if (pawn.ageTracker.CurLifeStageIndex == AgeStage.Toddler)
			pawn.Drawer.renderer.graphics.ResolveAllGraphics ();

			if (pawn.ageTracker.CurLifeStageIndex <= 1) {
				// Check if the baby is hungry, and if so, add the whiny baby hediff
				if (pawn.needs.food != null && (pawn.needs.food.CurLevelPercentage < pawn.needs.food.PercentageThreshHungry ||
					pawn.needs.joy.CurLevelPercentage < 0.1f) &&
				    !pawn.health.hediffSet.HasHediff (HediffDef.Named ("UnhappyBaby"))
				){
					Log.Message ("Adding unhappy baby hediff");
					pawn.health.AddHediff (HediffDef.Named ("UnhappyBaby"), null, null);
				}
			}
		}

		public override void Tick ()
		{
			if (pawn.Spawned) {
				if (pawn.IsHashIntervalTick (60)) {
					TickRare ();
				}
               // babystatetick += 4;
                //Log.Message("" + "baby state tick : " + babystatetick);
                //Log.Message("" + pawn.LabelIndefinite() + " Bio tick : " + pawn.ageTracker.AgeBiologicalTicks);
            }
        }

		public override bool Visible {
			get { return hediff_isOn; }
		}
	}
}

