using System.Collections.Generic;
using Verse;

namespace BabiesAndChildren.api
{
    /// <summary>
    /// Blacklist races to not have their children processed by this mod
    /// </summary>
    public static class Races
    {
        private static List<ThingDef> raceBlacklist = new List<ThingDef>();

        public static bool IsBlacklisted(ThingDef thing)
        {
            return raceBlacklist.Contains(thing);
        }

        public static bool Blacklist(ThingDef thing)
        {
            if (IsBlacklisted(thing) || thing == null)
                return false;
            raceBlacklist.Add(thing);
            return true;
        }

        public static bool UnBlacklist(ThingDef thing)
        {
            return raceBlacklist.Remove(thing);
        }
        
        
    }
}