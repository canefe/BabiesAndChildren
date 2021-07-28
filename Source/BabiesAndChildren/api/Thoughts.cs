using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace BabiesAndChildren.api
{
    /// <summary>
    /// Blacklist thoughts that babies and toddlers can have.
    /// </summary>
    public static class Thoughts
    {

        private static Dictionary<ThoughtDef, HashSet<int>> thoughtBlacklist = new Dictionary<ThoughtDef, HashSet<int>>();
        static Thoughts()
        {
            Blacklist(ThoughtDefOf.AteWithoutTable, AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDefOf.KnowPrisonerDiedInnocent, AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDefOf.KnowPrisonerSold, AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDefOf.Naked, AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDefOf.SleepDisturbed, AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDefOf.SleptOnGround, AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDef.Named("NeedOutdoors"), AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDef.Named("SleptInBarracks"), AgeStages.Baby, AgeStages.Toddler);
            Blacklist(ThoughtDef.Named("Expectations"), AgeStages.Baby, AgeStages.Toddler);

            if (ModsConfig.IdeologyActive) // ideology special thoughts
            {
                //TODO
            }


        }

        
        
        /// <summary>
        /// determines if thoughtDef is blacklisted for any AgeStage or any of the AgeStages specified
        /// </summary>
        /// <param name="thoughtDef">Def that is blacklisted</param>
        /// <param name="ageStages">optional list of AgeStages to check against</param>
        /// <returns>Whether thought is blacklisted for any AgeStage</returns>
        public static bool IsBlacklisted(ThoughtDef thoughtDef, params int[] ageStages)
        {
            
            if (!thoughtBlacklist.ContainsKey(thoughtDef))
            {
                return false;
            }
            if (ageStages.Length == 0)
            {
                ageStages = AgeStages.AllChildAgeStages;
            }

            return thoughtBlacklist[thoughtDef].Intersect(ageStages).Any();
            
            
        }
        

        public static bool Blacklist(ThoughtDef thoughtDef, params int[] ageStages)
        {

            if (thoughtDef == null)
            {
                return false;
            }
            if (ageStages.Length == 0)
            {
                ageStages = AgeStages.AllChildAgeStages;
            }
            thoughtBlacklist.Add(thoughtDef, new HashSet<int> {1, 2});
            thoughtBlacklist[thoughtDef].UnionWith(ageStages);
            return true;

        }

        public static bool UnBlacklist(ThoughtDef thoughtDef, params int[] ageStages)
        {
            return IsBlacklisted(thoughtDef) && thoughtBlacklist.Remove(thoughtDef);
        }
    }
}