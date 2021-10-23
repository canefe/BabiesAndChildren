using RimWorld;
using System;
using BabiesAndChildren.api;
using BabiesAndChildren.Tools;
using Verse;
using HealthUtility = BabiesAndChildren.Tools.HealthUtility;


namespace BabiesAndChildren
{
    /// <summary>
    /// This component is added and appropriately removed from pawns depending on their Age.
    /// It will keep track of a pawn's growth from birth to "adult" (see <see cref="AgeStage"/>).
    /// Adds and re-adds HeDiffs associated with pawn's age.
    ///
    /// See <see cref="Traits"/> for changing traits added at birth.
    /// </summary>
    class Growing_Comp : ThingComp
    {
        /// <summary>
        /// AgeStage pawn is currently set to (not necessarily the one it should be)
        /// </summary>
        /// 
        private int growthStage;
        
        /// <summary>
        /// Whether or not comp has been initialized 
        /// </summary>
        private bool initialized;

        /// <summary>
        /// Watch worktype settings
        /// *Mentor pawn
        /// *onlyMentor
        /// </summary>
        public Pawn mentor;
        public bool onlyMentor;
        
        public Pawn pawn => (Pawn) parent;
        public CompProperties_Growing Props => (CompProperties_Growing) props;

        private static readonly Backstory Childhood_Disabled = BackstoryDatabase.allBackstories["CustomBackstory_NA_Childhood_Disabled"];
        private static readonly Backstory Rimchild = BackstoryDatabase.allBackstories["CustomBackstory_Rimchild"];
        public override void PostExposeData()
        {
            Scribe_Values.Look(ref growthStage, "growthStage", 0);
            Scribe_Values.Look(ref initialized, "growthInitialized", false);
            Scribe_References.Look<Pawn>(ref mentor, "mentor", false);
            Scribe_Values.Look(ref onlyMentor, "onlyMentor", false);
            base.PostExposeData();
        }



        /// <summary>
        /// Setup or reinitialize pawn growth settings
        /// </summary>
        public void Initialize(bool reinitialize = false)
        {
            if (pawn.health.hediffSet.HasHediff(BnCHediffDefOf.BabyState0))
            {
                if (initialized && !reinitialize) return;
            }

            
            if (parent == null)
                Destroy();
            
            CLog.DevMessage((reinitialize ? "Reinitializing: " : "Initializing: ") + pawn.Name.ToStringShort);
            
            if (AgeStages.IsAgeStage(pawn, AgeStages.Baby))
            {
                InitBaby();
                if (!initialized)
                {
                    Birth();
                }
            }

            if (AgeStages.IsYoungerThan(pawn, AgeStages.Adult))
            {
                GrowToStage(AgeStages.GetAgeStage(pawn));
            }
            else
            {
                growthStage = AgeStages.GetAgeStage(pawn);
            }

            initialized = true;

        }
        
        public void Destroy()
        {
            if (parent == null) return;
            growthStage = AgeStages.Adult;
            DestroyHediffs();
        }

        /// <summary>
        /// Removes hediffs added for non adult pawns that simulate child behavior.
        /// </summary>
        public void DestroyHediffs()
        {
            if (parent == null) return;
            
            if (pawn.health.hediffSet.HasHediff(BnCHediffDefOf.BabyState0))
            {
                pawn.health.hediffSet.hediffs.Remove(pawn.health.hediffSet.GetFirstHediffOfDef(BnCHediffDefOf.BabyState0));
            }
            if (pawn.health.hediffSet.HasHediff(BnCHediffDefOf.UnhappyBaby))
            {
                pawn.health.hediffSet.hediffs.Remove(pawn.health.hediffSet.GetFirstHediffOfDef(BnCHediffDefOf.UnhappyBaby));
            }

        }

        public void InitBaby()
        {
            GrowToStage(AgeStages.GetAgeStage(pawn));
            HealthUtility.ClearImplantAndAddiction(pawn);
            pawn.style.beardDef = BeardDefOf.NoBeard;

            //For rabbie
            if (pawn.def.defName == "Rabbie")
            {
                HealthUtility.TryAddHediff(pawn, HediffDef.Named("PlanetariumAddiction"));
            }
           
        }

        public void Birth()
        {

            Pawn mother = pawn.GetMother();
            Pawn father = pawn.GetFather();
            MathTools.Fixed_Rand rand;
            if (mother != null)
            {
                rand = new MathTools.Fixed_Rand((int)mother.ageTracker.AgeBiologicalTicks);
                mother.ageTracker.AgeBiologicalTicks += 2;
                mother.ageTracker.AgeChronologicalTicks += 2;
            }
            else if (father != null)
            {
                rand = new MathTools.Fixed_Rand((int)father.ageTracker.AgeBiologicalTicks);
                father.ageTracker.AgeBiologicalTicks += 2;
                father.ageTracker.AgeBiologicalTicks += 2;
            }
            else
            {
                rand = new MathTools.Fixed_Rand((int)pawn.ageTracker.AgeBiologicalTicks);
            }
            if (rand.Fixed_RandChance(BnCSettings.STILLBORN_CHANCE))
            {
                BabyTools.Miscarry(pawn, mother, father);
                return;
            }


            if (mother != null)
            {
                HealthUtility.TryAddHediff(mother, HediffDef.Named("PostPregnancy"));
                HealthUtility.TryAddHediff(mother, HediffDef.Named("Lactating"),
                    HealthUtility.GetPawnBodyPart(mother, "Torso"));
                mother.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.IGaveBirth);
            }
            if (father != null)
            {
                father.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.PartnerGaveBirth);
                if (mother != null)
                {
                    father.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.WeHadBabies, mother, null);
                    mother.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.WeHadBabies, father, null);
                }
            }
            if (ChildrenBase.ModRimJobWorld_ON && BnCSettings.enable_postpartum)
            {
                HealthUtility.TryAddHediff(mother, HediffDef.Named("BnC_RJW_PostPregnancy"));
            }

            ChildrenUtility.PlayBabyCrySound(pawn);
            Props.ColonyBorn = true;
            pawn.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.JustBorn);
        }

        public void UpdateHediffs()
        {
            if (parent == null) return;
            //only toddlers and babies cry
            if ((growthStage <= AgeStages.Toddler) &&
                Hediff_UnhappyBaby.CheckUnhappy(pawn))
            {
                HealthUtility.TryAddHediff(pawn, BnCHediffDefOf.UnhappyBaby);
            }
            
            var currentBabyStateHediff = pawn.health.hediffSet.GetFirstHediffOfDef(BnCHediffDefOf.BabyState0);
            if (currentBabyStateHediff == null || currentBabyStateHediff.CurStageIndex != growthStage)
            {
                pawn.health.hediffSet.hediffs.Remove(currentBabyStateHediff);
                var growingHediff = HediffMaker.MakeHediff(BnCHediffDefOf.BabyState0, pawn);
                growingHediff.Severity = BnCHediffDefOf.BabyState0.stages[growthStage].minSeverity;
                pawn.health.AddHediff(growingHediff);
            }

        }


        public void UpdateAge()
        {
            if (parent == null) return;
            
            //TODO use percentages instead of years to allow for unique AgeStages 
            int age = pawn.ageTracker.AgeBiologicalYears;
            
            //Accelerated Growth Factor
            if (BnCSettings.accelerated_growth && (age < BnCSettings.accelerated_growth_end_age))
            {
                long acceleratedFactor = ChildrenUtility.SettingAcceleratedFactor(growthStage);
                //Can the biological age be greater than the chronological age?
                pawn.ageTracker.AgeBiologicalTicks += (acceleratedFactor) * 250;
                pawn.ageTracker.AgeChronologicalTicks += (acceleratedFactor) * 250;
            }
        }
        
        /// <summary>
        /// This method will setup the appropriate bodytype and backstory for a given agestage 
        /// </summary>
        /// <param name="stage">The new agestage</param>
        public void GrowToStage(int stage)
        {
            if (parent == null || growthStage == stage) return;

            growthStage = stage;
            
            
            StoryUtility.ChangeChildhood(pawn);
            
            MathTools.Fixed_Rand rand = new MathTools.Fixed_Rand(pawn);
            StoryUtility.ChangeBodyType(pawn, rand);

            bool initSize = !initialized;

            ChildrenUtility.TryDropInvalidEquipmentAndApparel(pawn);
            //update bodytype and backstory
            switch (growthStage)
            {
                case AgeStages.Baby:
                    break;
                case AgeStages.Toddler:
                    if (pawn.Faction != null && pawn.Faction.IsPlayer && initialized) { 
                        if (pawn.GetMother() != null)
                        {
                            pawn.GetMother().needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.MyChildGrowing);
                        }
                        if (pawn.GetFather() != null)
                        {
                            pawn.GetFather().needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.MyChildGrowing);
                        }
                    }
                    break;
                case AgeStages.Child:
                    if (pawn.Faction != null && pawn.Faction.IsPlayer && initialized)
                    {
                        if (pawn.GetMother() != null)
                        {
                            pawn.GetMother().needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.MyChildGrowing);
                        }
                        if (pawn.GetFather() != null)
                        {
                            pawn.GetFather().needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.MyChildGrowing);
                        }
                        Messages.Message("MessageGrewUpChild".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.PositiveEvent);
                    }
                    break;
                case AgeStages.Teenager:

                    if (pawn.Faction != null && pawn.Faction.IsPlayer && initialized)
                    {
                        if (pawn.GetMother() != null)
                        {
                            pawn.GetMother().needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.MyChildGrowing);
                        }
                        if (pawn.GetFather() != null)
                        {
                            pawn.GetFather().needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.MyChildGrowing);
                        }
                        Messages.Message("MessageGrewUpTeenager".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.PositiveEvent);
                    }
                    break;
            }
            ModTools.ChangeRJWHediffSeverity(pawn, initSize, rand);
        }


        public override void CompTickRare()
        {
            if(parent == null) return;
            
            if (!pawn.Spawned || pawn.Dead || growthStage == AgeStages.Adult) return;
            
            //Doing this here instead of PostSpawnSetup due to an odd null reference in MakeDowned when adding a hediff during setup
            //This is one way to ensure this only executes once the pawn is truly spawned
            Initialize();
            
            bool graphicsDirty = false;
            
            int ageStage = AgeStages.GetAgeStage(pawn);
            
            if (growthStage != ageStage)
            {
                graphicsDirty = true;
                PortraitsCache.SetDirty(pawn);
                GrowToStage(ageStage);
            }

            if (growthStage >= AgeStages.Adult)
            {
                Destroy();
                return;
            }

            if (ageStage == AgeStages.Baby)
            {
                if (pawn.story.childhood != Childhood_Disabled)
                {
                    StoryUtility.ChangeChildhood(pawn);
                }
            }

            //ugly way to do upright toddlers... really should just have had another age stage instead of this bs.
            if (ageStage == AgeStages.Toddler)
            {
                if (pawn.story.bodyType != BodyTypeDefOf.Thin)
                {
                    if (ChildrenUtility.ToddlerIsUpright(pawn))
                    {
                        StoryUtility.ChangeBodyType(pawn);
                        graphicsDirty = true;
                    }
                }
            }
            
            UpdateHediffs();
            
            UpdateAge();
            

            if (graphicsDirty)
            {
                pawn.Drawer.renderer.graphics.ResolveAllGraphics(); 
            } 
        }

    }

    /// <summary>
    /// This properties class is necessary to reference Growing_Comp when adding the comp to a child, but
    /// is otherwise not used.
    /// geneticTraits may be set before a call to initialize, and the traits will be applied 
    /// to a new born pawn on initialization based on random weight.
    /// </summary>
    public class CompProperties_Growing : CompProperties
    {
        public bool ColonyBorn { get; set; }
        public CompProperties_Growing()
        {
            this.compClass = typeof(Growing_Comp);
        }
        public CompProperties_Growing(Type compClass) : base(compClass)
        {
        }

    }
}