using System;
using System.Collections.Generic;
using System.Linq;
using BabiesAndChildren.Tools;
using RimWorld;
using Verse;
using HealthUtility = BabiesAndChildren.Tools.HealthUtility;

namespace BabiesAndChildren
{
    public static class BabyTools
    {
        public static void Miscarry(Pawn baby, Pawn mother, Pawn father)
        {
            baby.Name = new NameSingle("Unnamed".Translate(), false);
            baby.SetFaction(null, null);
            HealthUtility.TryAddHediff(baby, HediffDef.Named("DefectStillborn"), force: true);
            if (father != null)
            {
                father.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("BabyStillborn"), baby);
                RemoveChildDiedThought(father, baby);
            }

            if (mother != null)
            {
                mother.needs.mood.thoughts.memories.TryGainMemory(ThoughtDef.Named("BabyStillborn"), baby);
                RemoveChildDiedThought(mother, baby);
                Find.LetterStack.ReceiveLetter("WordStillborn".Translate(), TranslatorFormattedStringExtensions.Translate("MessageStillborn", mother.LabelIndefinite()), LetterDefOf.Death, mother);
            }
        }

        /// <summary>
        /// Sets a SkillRecord passion in similar fashion to vanilla
        /// </summary>
        /// <param name="skillRecord"></param>
        /// <param name="rand"></param>
        /// <param name="always">Whether a passion should always be enabled</param>
        public static void SetPassion(SkillRecord skillRecord, MathTools.Fixed_Rand rand, bool always = false)
        {
            double threshold =  skillRecord.Level * 0.11;
            double randomDouble = rand.Fixed_RandDouble(0, 1);
            if (always || (randomDouble < threshold))
                skillRecord.passion = randomDouble >=  threshold * 0.2 ? Passion.Minor : Passion.Major;
            else
                skillRecord.passion = Passion.None;

        }
        
        /// <summary>
        /// Sets a pawn's skills and passions based on:
        /// 1. mother and father's skills
        /// 2. pawn's current traits
        /// </summary>
        /// <param name="pawn">pawn whose skills and passions will be set</param>
        /// <param name="mother">pawn's mother</param>
        /// <param name="father">pawn's father</param>
        /// <param name="rand">random number generator</param>
        public static void SetBabySkillsAndPassions(Pawn pawn, Pawn mother, Pawn father, MathTools.Fixed_Rand rand)
        {

            if (father == null || mother == null)
            {
                CLog.Warning("Father or Mother is null. Randomly generating skills.");
                VerseExposed.PawnGenerator_GenerateSkills(pawn);
                return;
            }


            if (BnCSettings.baby_Inherit_percentage == BnCSettings.BabyInheritPercentageHandleEnum._Random)
            {
                VerseExposed.PawnGenerator_GenerateSkills(pawn);
                return;
            }
            
            //change skills and passions based on mommy and daddy
            foreach (var skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                    
                int motherSkillLevel = mother.skills.GetSkill(skillDef).Level;
                CLog.DevMessage("mother's " + skillDef.defName + " Skills level =" + motherSkillLevel);

                int fatherSkillLevel = father.skills.GetSkill(skillDef).Level;
                CLog.DevMessage("father's " + skillDef.defName + " Skills level =" + fatherSkillLevel);

                SkillRecord babySkillRecord = pawn.skills.GetSkill(skillDef);
                
                if (babySkillRecord.TotallyDisabled)
                    continue;
                
                //baby's skill level is genetic
                int babySkillLevel = MathTools.Avg(fatherSkillLevel, motherSkillLevel);
                    
                //TODO have setting be variable instead of a dropdown
                switch (BnCSettings.baby_Inherit_percentage)
                {
                    case BnCSettings.BabyInheritPercentageHandleEnum._None:
                        break;

                    case BnCSettings.BabyInheritPercentageHandleEnum._25:
                        babySkillLevel = (int)Math.Floor(babySkillLevel * 0.25);
                        break;

                    case BnCSettings.BabyInheritPercentageHandleEnum._50:
                        babySkillLevel = (int)Math.Floor(babySkillLevel * 0.5);
                        break;

                    case BnCSettings.BabyInheritPercentageHandleEnum._75:
                        babySkillLevel = (int)Math.Floor(babySkillLevel * 0.75);
                        break;

                    case BnCSettings.BabyInheritPercentageHandleEnum._90:
                        babySkillLevel = (int)Math.Floor(babySkillLevel * 0.90);
                        break;

                }
                
                //randomize the final level a bit               
                babySkillLevel = (int) (babySkillLevel * rand.Fixed_RandDouble(0.7, 1.1));
               
                //level up
                babySkillRecord.EnsureMinLevelWithMargin(babySkillLevel);

                //add between 10% and 90% of xp required for next level up
                babySkillRecord.xpSinceLastLevel += (float)rand.Fixed_RandDouble(babySkillRecord.XpRequiredForLevelUp * 0.1, babySkillRecord.XpRequiredForLevelUp * 0.9);
               
                SetPassion(babySkillRecord, rand);
                
                CLog.DevMessage("" + pawn.Name + "'s " + skillDef.defName + " Skills set =" + pawn.skills.GetSkill(skillDef));

            }
            //change skills and passions based on pawn's current traits
            foreach(Trait trait in pawn.story.traits.allTraits) 
            {
                //enable forced passions
                foreach (SkillDef skillDef in trait.def.forcedPassions)
                {
                    SetPassion(pawn.skills.GetSkill(skillDef), rand, true);
                }
                
                //disable conflicting passions
                foreach (SkillDef skillDef in trait.def.conflictingPassions)
                {
                    pawn.skills.GetSkill(skillDef).passion = Passion.None;
                }

                if (trait.CurrentData?.skillGains == null)
                    continue;
                
                //add levels based on trait's skillGains
                foreach (var  kvp in trait.CurrentData.skillGains)
                {
                    
                    SkillRecord babySkillRecord = pawn.skills.GetSkill(kvp.Key);
                    babySkillRecord.Level += kvp.Value;
                }


            }
            
        }
        /// <summary>
        /// Sets the babies traits either randomly or based on mother and father
        /// depending on mod settings and luck
        /// </summary>
        public static void SetBabyTraits(Pawn pawn, Pawn mother, Pawn father, MathTools.Fixed_Rand rand)
        {
            //clear traits
            pawn.story.traits.allTraits.Clear();
            
            if (mother == null || father == null)
            {
                CLog.Warning("Mother or father of: " + pawn.Name.ToStringShort + " is null, generating random traits.");
                VerseExposed.PawnGenerator_GenerateTraits(pawn);
                return;
            }

            if (BnCSettings.baby_Inherit_percentage == BnCSettings.BabyInheritPercentageHandleEnum._Random)
            {
                VerseExposed.PawnGenerator_GenerateTraits(pawn);
                return;
            }

            //add new type and sexuality
            StoryUtility.GetNewTypeAndSexuality(pawn, rand);
            
            if (BnCSettings.baby_Inherit_percentage == BnCSettings.BabyInheritPercentageHandleEnum._None)
                return;

            int fatherTraitCount = father.story.traits.allTraits.Count;
            List<Trait> fatherTraitList = father.story.traits.allTraits;
            
            if (fatherTraitCount <= 0)
                CLog.DevMessage("Father has no traits!");

            int motherTraitCount = mother.story.traits.allTraits.Count;
            List<Trait> motherTraitList = mother.story.traits.allTraits;
            if (motherTraitCount <= 0)
                CLog.DevMessage("Mother has no traits!");

            if (BnCSettings.debug_and_gsetting)
            {
                foreach (var trait in fatherTraitList)
                {
                    CLog.DevMessage("Father trait: " + trait.def.defName);
                }

                foreach (var trait in motherTraitList)
                {
                    CLog.DevMessage("Mother trait: " + trait.def.defName);
                }
            }

            double inheritChance = 1;
            switch (BnCSettings.baby_Inherit_percentage)
            {
                case BnCSettings.BabyInheritPercentageHandleEnum._90:
                    inheritChance = 0.9 + 0.1;
                    break;
                case BnCSettings.BabyInheritPercentageHandleEnum._75:
                    inheritChance = 0.75 + 0.1;
                    break;
                case BnCSettings.BabyInheritPercentageHandleEnum._50:
                    inheritChance = 0.5 + 0.1;
                    break;
                case BnCSettings.BabyInheritPercentageHandleEnum._25:
                    inheritChance = 0.25 + 0.1;
                    break;
            }


            StoryUtility.InheritTraits(pawn, mother, rand, inheritChance);
            StoryUtility.InheritTraits(pawn, father, rand, inheritChance + 0.2);

            //Add more traits for some reason
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (BnCSettings.MAX_TRAIT_COUNT > 2)
            {
                // give random trait
                if (pawn.story.traits.allTraits.Count == 2)
                {
                    if (rand.Fixed_RandChance(0.15)) StoryUtility.GiveARandomTrait(pawn, rand);
                }
                else if (pawn.story.traits.allTraits.Count <= 1)
                {
                    if (rand.Fixed_RandChance(0.4)) StoryUtility.GiveARandomTrait(pawn, rand);
                    if (rand.Fixed_RandChance(0.15)) StoryUtility.GiveARandomTrait(pawn, rand);
                }
            }
            
        }
        

        public static void RemoveChildDiedThought(Pawn pawn, Pawn child)
        {
            ThoughtDef sonDiedThought = ThoughtDef.Named("MySonDied");
            ThoughtDef daughterDiedThought = ThoughtDef.Named("MyDaughterDied");
            ThoughtDef pawnWithGoodOpinionDiedThought = ThoughtDef.Named("PawnWithGoodOpinionDied");
            ThoughtDef witnessedDeathFamilyThought = ThoughtDef.Named("WitnessedDeathFamily");
            ThoughtDef witnessedDeathNonAllyThought = ThoughtDef.Named("WitnessedDeathNonAlly");
            
            MemoryThoughtHandler memories = pawn.needs.mood.thoughts.memories;

            if (memories.NumMemoriesOfDef(sonDiedThought) <= 0 &&
                memories.NumMemoriesOfDef(daughterDiedThought) <= 0) return;
            
            foreach (Thought_Memory thought in memories.Memories.ToList())
            {
                if ((thought.def == sonDiedThought || 
                     thought.def == daughterDiedThought || 
                     thought.def == pawnWithGoodOpinionDiedThought ) &&
                    thought.otherPawn == child)
                {
                    memories.Memories.Remove(thought);
                }
                if (thought.def == witnessedDeathFamilyThought || 
                    thought.def == witnessedDeathNonAllyThought)
                {
                    memories.Memories.Remove(thought);
                }
            }
        }
    }

}