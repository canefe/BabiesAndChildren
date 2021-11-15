using System.Collections.Generic;
using RimWorld;

namespace BabiesAndChildren.api {
    /// <summary>
    /// Add traits to be be inherited at birth.
    /// </summary>
    public static class Traits {

        private static List<TraitDef> geneticTraits = new List<TraitDef>();

        static Traits() {
            //Genetic Traits
            RegisterGeneticTrait(TraitDefOf.Psychopath);
            RegisterGeneticTrait(TraitDefOf.SpeedOffset);
            RegisterGeneticTrait(TraitDefOf.Tough);
            RegisterGeneticTrait(TraitDefOf.Beauty);
            RegisterGeneticTrait(TraitDefOf.Industriousness);
            RegisterGeneticTrait(TraitDefOf.TooSmart);
            RegisterGeneticTrait(TraitDefOf.DrugDesire);
            RegisterGeneticTrait(TraitDefOf.NaturalMood);
            RegisterGeneticTrait(TraitDefOf.Nerves);
            RegisterGeneticTrait(TraitDefOf.PsychicSensitivity);
            RegisterGeneticTrait(TraitDefOf.AnnoyingVoice);
            RegisterGeneticTrait(TraitDefOf.CreepyBreathing);
            RegisterGeneticTrait(TraitDefOf.GreatMemory);
            RegisterGeneticTrait(TraitDefOf.Greedy);
            RegisterGeneticTrait(TraitDefOf.Jealous);
            RegisterGeneticTrait(TraitDefOf.Kind);
            RegisterGeneticTrait(TraitDef.Named("Immunity"));
            RegisterGeneticTrait(TraitDef.Named("Neurotic"));
            RegisterGeneticTrait(TraitDef.Named("QuickSleeper"));
            RegisterGeneticTrait(TraitDef.Named("Wimp"));
        }

        /// <summary>
        /// Register a trait to have it considered a genetically inheritable trait passed 
        /// from parent to child. Pawns may be born with these traits.
        /// </summary>
        /// <param name="def">New trait to be added</param>
        public static void RegisterGeneticTrait(TraitDef def) {
            if (!geneticTraits.Contains(def)) {
                geneticTraits.Add(def);
            }
        }

        /// <summary>
        /// Unregister a genetic trait so it is not a valid inheritable trait.
        /// </summary>
        /// <param name="def">New trait to be removed</param>
        public static void UnRegisterGeneticTrait(TraitDef def) {
            if (geneticTraits.Contains(def)) {
                geneticTraits.Remove(def);
            }
        }

        /// <summary>
        /// Retrieve a list of genetic traits
        /// </summary>
        /// <returns>a list of genetic traits</returns>
        public static List<TraitDef> GetGeneticTraits() {
            return new List<TraitDef>(geneticTraits);
        }

        /// <summary>
        /// Checks if a given traitdef is listed as a genetic trait
        /// </summary>
        public static bool IsGeneticTrait(TraitDef def)
        {
            return geneticTraits.Contains(def);
        }
    }
}
