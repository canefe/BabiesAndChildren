using System.Collections.Generic;
using AlienRace;
using BabiesAndChildren.api;
using RimWorld;
using Verse;

namespace BabiesAndChildren.Tools
{
    /// <summary>
    /// Helper class for pawn story records
    /// </summary>
    public static class StoryUtility
    {
        private static readonly Backstory Childhood_Disabled = BackstoryDatabase.allBackstories["CustomBackstory_NA_Childhood_Disabled"];
        private static readonly Backstory Rimchild = BackstoryDatabase.allBackstories["CustomBackstory_Rimchild"];
        
        /// <summary>
        /// This method will roll for a chance to make the pawn Asexual, Bisexual, or Gay
        /// </summary>
        /// <param name="pawn">The pawn to be analyzed</param>
        /// <param name="rand">fixed random number generator</param>
        /// <returns>An AcquirableTrait for sexuality</returns>
        public static void GetNewTypeAndSexuality(Pawn pawn, MathTools.Fixed_Rand rand)
        {
            int traitsCount = pawn.story.traits.allTraits.Count;
            if (traitsCount >= BnCSettings.MAX_TRAIT_COUNT) return;
            
            if (rand.Fixed_RandChance(BnCSettings.GET_NEW_TYPE_CHANCE))
            {
                pawn.story.traits.GainTrait(new Trait(BnCTraitDefOf.Newtype, 0, true));
                Find.LetterStack.ReceiveLetter("Newtype".Translate(), 
                    "MessageNewtype".Translate(pawn.LabelIndefinite()), 
                    LetterDefOf.PositiveEvent, pawn);
            }

            if (rand.Fixed_RandChance(BnCSettings.GET_SPECIFIC_SEXUALITY))
            {
                pawn.story.traits.GainTrait(rand.Fixed_RandBool()
                    ? new Trait(TraitDefOf.Asexual, 0, true)
                    : new Trait(TraitDefOf.Bisexual, 0, true));
            }
            else if (rand.Fixed_RandChance(BnCSettings.GET_GAY_SEXUALITY))
            {
                pawn.story.traits.GainTrait(new Trait(TraitDefOf.Gay, 0, true));
            }
        }

        /// <summary>
        /// This method will check trait conflict, already exist
        /// and no conflict will apply pawn trait
        /// </summary>
        /// <returns>Whether the trait was applied</returns>
        public static bool ApplyTraitToPawn(Pawn pawn, Trait traitToGive, MathTools.Fixed_Rand rand, double chance)
        {
            if ((traitToGive == null) || (rand.Fixed_RandDouble(0, 1) > chance) ||  pawn.story.traits.HasTrait(traitToGive.def)) 
                return false;

            foreach (Trait itrait in pawn.story.traits.allTraits)
            {
                if (traitToGive.def.ConflictsWith(itrait))
                    return false;
            }

            pawn.story.traits.GainTrait(traitToGive);
            return true;
        }

        /// <summary>
        /// Tries to apply a random trait from Traits.GetGeneticTraits()
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="rand"></param>
        /// <returns>whether a trait was added</returns>
        public static bool GiveARandomTrait(Pawn pawn, MathTools.Fixed_Rand rand)
        {
            List<TraitDef> geneticTraits = Traits.GetGeneticTraits();

            int attempts = 0;
            int maxAttempts = geneticTraits.Count;
            
            while(attempts++ < maxAttempts)
            {
                TraitDef def = (TraitDef) rand.Fixed_RandElement(geneticTraits);
                Trait trait2 = new Trait(def, PawnGenerator.RandomTraitDegree(def), false);
                if (StoryUtility.ApplyTraitToPawn(pawn, trait2,rand, 1)) 
                    return true;                
            }

            return false;
        }

        /// <summary>
        /// Adds traits from parent to child based on inheritChance, mod settings, and luck.
        /// </summary>
        public static void InheritTraits(Pawn child, Pawn parent, MathTools.Fixed_Rand rand, double inheritChance)
        {
            if (child == null || parent == null)
                return;

            if (parent.story.traits.allTraits.Count <= 0 || child.story.traits.allTraits.Count > BnCSettings.MAX_TRAIT_COUNT)
                return;
            
            List<Trait> parentTraits = new List<Trait>();

            foreach (Trait trait in parent.story.traits.allTraits)
            {
                if (Traits.IsGeneticTrait(trait.def))
                {
                    parentTraits.Add(trait);
                }
            }
            
            if(parentTraits.Count <= 0) return;
            

            int traitsToGive = rand.Fixed_RandInt(1, 2);
            int traitsAdded = 0;
            
            int attempts = 0;
            int maxAttempts = parentTraits.Count;
            
            while ((traitsAdded < BnCSettings.MAX_TRAIT_COUNT) &&
                   (traitsAdded < traitsToGive) &&
                   (attempts++ < maxAttempts))
            {
                var traitToGive = (Trait) rand.Fixed_RandElement(parentTraits);
                if (StoryUtility.ApplyTraitToPawn(child, traitToGive, rand, inheritChance))
                {
                    traitsAdded++;
                }

            }

        }

        public static bool TrySetPawnBodyType(Pawn pawn, BodyTypeDef bodyTypeDef, bool force = false)
        {
            
            if (RaceUtility.IsHuman(pawn) || force)
            {
                pawn.story.bodyType = bodyTypeDef;
                return true;
            } 
            if (pawn.def is ThingDef_AlienRace thingDef)
            {
                List<BodyTypeDef> bodyTypes = thingDef.alienRace.generalSettings.alienPartGenerator.alienbodytypes;
                if (bodyTypes.NullOrEmpty())
                    return false;

                if (bodyTypes.Contains(bodyTypeDef))
                {
                    pawn.story.bodyType = bodyTypeDef;
                    return true;
                }

                //can't set to desired to body type but leaving an invalid one
                //leads to pink boxes
                if (!bodyTypes.Contains(pawn.story.bodyType))
                {
                    pawn.story.bodyType = bodyTypes.RandomElement<BodyTypeDef>();
                    return false;
                }
                
            }
            //pawn's which are not humans or alien races is out of the scope of this mod
            return false;
        }
        

        /// <summary>
        /// Changes a pawn's childhood and if it's childhood is changed it's
        /// work types and disabled skills will be updated.
        /// Will also update childhood for children based on AgeStage if no new childhood is provided.
        /// </summary>
        /// <returns>Whether the childhood changed</returns>
        public static bool ChangeChildhood(Pawn pawn, Backstory newChildhood = null)
        {
            
            if (pawn == null) return false;

            Backstory currentChildhood = pawn.story.childhood;
            
            var comp = pawn.TryGetComp<Growing_Comp>();
            if (newChildhood == null)
            {
                switch (AgeStages.GetAgeStage(pawn))
                {
                    case AgeStages.Baby:
                        newChildhood = Childhood_Disabled;
                        break;
                    case AgeStages.Toddler:
                        goto case AgeStages.Baby;
                    case AgeStages.Child:
                        if (currentChildhood == Childhood_Disabled)
                        {
                            newChildhood = Rimchild;
                        }
                        break;
                    case AgeStages.Teenager:
                        goto case AgeStages.Child;
                    case AgeStages.Adult:
                        goto case AgeStages.Child;
                    
                }
            }

            if (newChildhood == null || newChildhood == currentChildhood) 
                return false;
            
            pawn.story.childhood = newChildhood;
            pawn.Notify_DisabledWorkTypesChanged();
            pawn.skills.Notify_SkillDisablesChanged();
            MeditationFocusTypeAvailabilityCache.ClearFor(pawn);
            return true;

        }

        /// <summary>
        /// Changes pawn's body type based on it's AgeStage
        /// </summary>
        /// <param name="pawn">pawn to be altered</param>
        /// <param name="rand">random number generator</param>
        public static void ChangeBodyType(Pawn pawn, MathTools.Fixed_Rand rand = null)
        {
            if (pawn == null) return;
            bool force = false;
            
            
            BodyTypeDef newBodyType = BodyTypeDefOf.Thin;
            switch (AgeStages.GetAgeStage(pawn))
            {
                case AgeStages.Baby: 
                    newBodyType = BodyTypeDefOf.Fat;
                    force = true;
                    break;
                
                case AgeStages.Toddler:
                    newBodyType = ChildrenUtility.ToddlerIsUpright(pawn) ? BodyTypeDefOf.Thin : BodyTypeDefOf.Fat;
                    force = true;
                    break;
                
                case AgeStages.Child:
                    newBodyType = BodyTypeDefOf.Thin;
                    break;

                case AgeStages.Teenager:
                    if (rand == null)
                        rand = new MathTools.Fixed_Rand(pawn);
                    
                    if (rand.Fixed_RandChance(0.35f))
                    {
                        newBodyType = BodyTypeDefOf.Thin;
                    }
                    else if(pawn.gender == Gender.Male)
                    {
                        newBodyType = BodyTypeDefOf.Male;
                    }
                    else if (pawn.gender == Gender.Female)
                    {
                        newBodyType= BodyTypeDefOf.Female;
                    }
                    break;
                case AgeStages.Adult:
                    return;
            }

            if (force && !RaceUtility.ThingUsesChildren(pawn.def))
            {
                force = false;
            }

            bool result = TrySetPawnBodyType(pawn, newBodyType, force);
            if (result)
                CLog.DevMessage(pawn.Name.ToStringShort + " bodyType was changed to: " + newBodyType);
            else
                CLog.DevMessage(pawn.Name.ToStringShort + " bodyType failed to change to: " + newBodyType);
        }
    }
}