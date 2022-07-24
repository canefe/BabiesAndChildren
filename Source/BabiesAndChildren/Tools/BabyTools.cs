using System;
using System.Collections.Generic;
using System.Linq;
using BabiesAndChildren.Tools;
using Verse;
using RimWorld;
using HealthUtility = BabiesAndChildren.Tools.HealthUtility;

namespace BabiesAndChildren
{
    public static class BabyTools
    {
        public static void Miscarry(Pawn baby, Pawn mother, Pawn father)
        {
            HealthUtility.TryAddHediff(baby, BnCHediffDefOf.DefectStillborn, force: true);
            if (father != null)
            {
                father.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.BabyStillborn, baby);
                RemoveChildDiedThought(father, baby);
            }

            if (mother != null)
            {
                mother.needs.mood.thoughts.memories.TryGainMemory(BnCThoughtDefOf.BabyStillborn, baby);
                RemoveChildDiedThought(mother, baby);
                Find.LetterStack.ReceiveLetter("WordStillborn".Translate(), TranslatorFormattedStringExtensions.Translate("MessageStillborn", mother.LabelIndefinite()), LetterDefOf.Death, mother);
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