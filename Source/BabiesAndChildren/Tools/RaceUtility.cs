using System.Collections.Generic;
using System.Linq;
using BabiesAndChildren.api;
using Verse;
using RimWorld;

namespace BabiesAndChildren.Tools
{
    /// <summary>
    /// Helper class for race properties
    /// </summary>
    public static class RaceUtility
    {

        private static Dictionary<ThingDef, bool> thingUsesChildrenCache = new Dictionary<ThingDef, bool>();
        private static Dictionary<ThingDef, AlienChildDef> alienChildDefCache = new Dictionary<ThingDef, AlienChildDef>();

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
            
            if (thingUsesChildrenCache.TryGetValue(thingDef, out var usesChildren))
            {
                return usesChildren;
            }
            usesChildren = 
                   !ModTools.IsRobot(thingDef) &&
                   !Races.IsBlacklisted(thingDef);
            
            CLog.DevMessage(thingDef.defName + " cached as " + usesChildren + " in ThingUsesChildren");
            thingUsesChildrenCache[thingDef] = usesChildren;
            if (ChildrenBase.ModHAR_ON && CacheAlienChildDef(thingDef))
            {
                CLog.DevMessage(thingDef.defName + " AlienChildDef cached");
                usesChildren = GetAlienChildDef(thingDef) != null ? (GetAlienChildDef(thingDef).disabled ? false : usesChildren) : usesChildren;
            }
            return usesChildren;
        }

        public static AlienChildDef GetAlienChildDef(ThingDef race)
        {
            if (alienChildDefCache.TryGetValue(race, out var childDef))
            {
                return childDef;
            }
            return null;
        }

        public static bool CacheAlienChildDef(ThingDef thingDef)
        {
            if (DefDatabase < AlienChildDef >.GetNamed(thingDef.defName, false) != null)
            {
                alienChildDefCache[thingDef] = DefDatabase<AlienChildDef>.GetNamed(thingDef.defName, false);
                return true;
            }
            return false;
        }

        public static void ClearCache()
        {
            thingUsesChildrenCache.Clear();
            alienChildDefCache.Clear();
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
            return pawn.def == ThingDefOf.Human;
        }
        

        public static bool HasHumanlikeHead(Pawn pawn)
        {
            //TODO have a setting to chose which races are effected by this
            string[] humanlikes = { "Kurin_Race", "Ratkin"};
            return humanlikes.Contains(pawn.def.defName);
        }
    }
}