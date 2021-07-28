using System.Collections.Generic;
using System.Linq;
using BabiesAndChildren.api;
using Verse;

namespace BabiesAndChildren.Tools
{
    /// <summary>
    /// Helper class for race properties
    /// </summary>
    public static class RaceUtility
    {

        private static Dictionary<ThingDef, bool> thingUsesChildrenCache = new Dictionary<ThingDef, bool>();


        public static bool ThingUsesChildren(Thing thing)
        {
            return ThingUsesChildren(thing?.def);
        }
        /// <summary>
        /// Whether a thing with race will have it's children handled by this mod.
        /// </summary>
        public static bool ThingUsesChildren(ThingDef thingDef)
        {
            var raceProps = thingDef?.race;
            if (raceProps == null || !raceProps.Humanlike)
                return false;
            
            if (thingUsesChildrenCache.ContainsKey(thingDef))
                return thingUsesChildrenCache[thingDef];
            var usesChildren = 
                   !ModTools.IsRobot(thingDef) &&
                   !Races.IsBlacklisted(thingDef);
            
            CLog.DevMessage(thingDef.defName + " cached as " + usesChildren + " in ThingUsesChildren");
            thingUsesChildrenCache[thingDef] = usesChildren;
            return usesChildren;
        }

        public static void ClearCache()
        {
            thingUsesChildrenCache.Clear();
        }


        /// <summary>
        /// Determines whether a pawn's race has it's children handled by this mod
        /// </summary>
        public static bool PawnUsesChildren(Pawn pawn)
        {
            return ThingUsesChildren(pawn?.def);
        }

        public static bool IsHuman(Pawn pawn)
        {
            return pawn.def == DefDatabase<ThingDef>.GetNamed("Human");
        }
        

        public static bool HasHumanlikeHead(Pawn pawn)
        {
            //TODO have a setting to chose which races are effected by this
            string[] humanlikes = { "Kurin_Race", "Ratkin"};
            return humanlikes.Contains(pawn.def.defName);
        }
    }
}