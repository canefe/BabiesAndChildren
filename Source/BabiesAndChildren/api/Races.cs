using System.Collections.Generic;
using Verse;
using BabiesAndChildren.Tools;

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
    public class RaceSettings : IExposable
    {
        public void ExposeData()
        {
            Scribe_Values.Look<string>(ref defName, "defName", null);
            Scribe_Values.Look<float>(ref sizeModifier, "sizeModifier", 1f);
            Scribe_Values.Look<float>(ref headOffset, "headOffset", 1f);
            Scribe_Values.Look<float>(ref hairSizeModifier, "hairSizeModifier", 1f);
            Scribe_Values.Look<float>(ref headSizeModifier, "headSizeModifier", 1f);
            Scribe_Values.Look<bool>(ref scaleChild, "scaleChild", true);
            Scribe_Values.Look<bool>(ref scaleTeen, "scaleTeen", true);
        }

        public string defName;

        public float sizeModifier = 1f;

        public float headOffset = 1f;

        public float hairSizeModifier = 1f;

        public float headSizeModifier = 1f;

        public bool scaleChild = true;

        public bool scaleTeen = true;

    }
}