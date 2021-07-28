using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using LifeStageUtility = BabiesAndChildren.Tools.LifeStageUtility;

namespace BabiesAndChildren.api
{
    /// <summary>
    /// AgeStages are objects containing information related to the stages of childhood for a pawn.
    /// Baby are useless requiring food and attention
    /// Toddlers unlike babies can move
    /// Children are basically little pawns with some special behavior
    /// Teenagers are the same as adults except may be rendered a little smaller
    /// Adults are what a race is by default
    /// 
    /// Note: AgeStages are different from LifeStages. They originally were just a reference to an index in a race's LifeStages
    /// collection but LifeStages differ to much from race to race for this to be reliable.
    /// Since Life Stages are independent from AgeStages issues may arise where there are duplicate
    /// effects for children but imo these are easier to resolve then writing patches for every race with less
    /// than 5 life stages.
    ///
    /// Dynamically adding life stages would be another solution but that seems more difficult than this approach.
    ///
    /// Races that become adults too soon or stay children too long can be changed by adding
    /// mod def extensions instead of redoing their life stages.
    /// 
    /// Changing ageStagePercents will change how long a each part of a race's childhood is
    /// changing minAgeAdult and minAgeBaby will change how long a pawn's childhood is and when it happens
    /// in the case of minAgeBaby
    /// </summary>
    public class AgeStages : ICloneable
    {
        //The AgeStages
        public const int Baby = 0;
        public const int Toddler = 1;
        public const int Child = 2;
        public const int Teenager = 3;
        public const int Adult = 4;

        public static readonly int[] AllChildAgeStages =
        {
            Baby,
            Toddler,
            Child,
            Teenager
        };

        //Cache of what race uses what AgeStages instance
        private static Dictionary<ThingDef, AgeStages> thingAgeStagesMap = new Dictionary<ThingDef, AgeStages>();
        
        private const int OneHundredPercent = 100;
        private const int ZeroPercent = 0;

        /// <summary>
        /// What percentage of childhood is dedicated to each AgeStage
        /// </summary>
        public int[] ageStagePercents =
        {
            15,
            15,
            40,
            30
        };
        /// <summary>
        /// When a pawn's childhood ends
        /// </summary>
        public float minAgeAdult = 18f;
        
        /// <summary>
        /// When a pawn's childhood begins
        /// </summary>
        public float minAgeBaby = 0f;
        
        /// <summary>
        /// Creates an AgeStages instance defined by modextensions
        /// <see cref="BabiesAndChildren.AgeStagePercents"/>
        /// <see cref="BabiesAndChildren.MinAgeAdult"/>
        /// <see cref="BabiesAndChildren.MinAgeBaby"/>
        /// </summary>
        /// <param name="thing">Def of thing with race</param>
        public AgeStages(ThingDef thing)
        {

            if (thing == null)
                return;
            //determine the minAgeAdult value or leave the default 18
            //via extension
            MinAgeAdult minAgeAdultExtension = thing.GetModExtension<MinAgeAdult>();
            if (minAgeAdultExtension != null)
            {
                minAgeAdult = minAgeAdultExtension.minAgeAdult;
            }
            else
            {   // via searching the lifestages for the word adult
                LifeStageAge stage = LifeStageUtility.GetLifeStageAgeContaining(thing, "Adult");
                if (stage?.minAge != null)
                {
                    minAgeAdult = stage.minAge;
                }
                // via half the average pawn age (avg human age is 36)
                else if (thing.race?.ageGenerationCurve != null)
                {
                    minAgeAdult = Rand.ByCurveAverage(thing.race.ageGenerationCurve) / 2.0f;
                }
            }

            MinAgeBaby minAgeBabyExtension = thing.GetModExtension<MinAgeBaby>();
            if (minAgeBabyExtension != null)
            {
                minAgeBaby = minAgeBabyExtension.minAgeBaby;
            }
            //Can change the percent of time in each childhood stage
            AgeStagePercents ageStagePercentsExtension = thing.GetModExtension<AgeStagePercents>();
            if (ageStagePercentsExtension != null)
            {
                ageStagePercents = ageStagePercentsExtension.ageStagePercents;
            }

        }
        /// <summary>
        /// Copy an AgeStages instance
        /// </summary>
        public AgeStages(AgeStages other)
        {
            this.ageStagePercents = new int[other.ageStagePercents.Length];
            for (int i = 0; i < other.ageStagePercents.Length; i++)
            {
                this.ageStagePercents[i] = other.ageStagePercents[i];
            }

            this.minAgeAdult = other.minAgeAdult;
            this.minAgeBaby = other.minAgeBaby;
        }
        
        /// <summary>
        /// Gets a copy of an AgeStages mapped to thing
        /// </summary>
        /// <returns>null if no race in thing</returns>
        public static AgeStages GetAgeStages(ThingDef thing)
        {
            if (thing.race == null)
                return null;
            
            if (!thingAgeStagesMap.ContainsKey(thing))
            { 
                var ageStages = new AgeStages(thing);
                var isValid = ageStages.IsValid();
                CLog.DevMessage("Generated " + (isValid ? "Valid" : "Invalid") + " AgeStages for: " + thing.defName +
                                " with minAgeBaby: " + ageStages.minAgeBaby +
                                " and minAgeAdult: " + ageStages.minAgeAdult);

                if (!isValid)
                {
                    ageStages = null;
                }
                
                thingAgeStagesMap[thing] = ageStages;
            }
            return thingAgeStagesMap[thing];
            
        }

        public static AgeStages GetAgeStages(Pawn pawn)
        {
            return GetAgeStages(pawn.def);
        }

        /// <summary>
        /// Set the AgeStages used by thing 
        /// </summary>
        public static bool SetAgeStages(ThingDef thing, AgeStages ageStages)
        {
            if (ageStages == null || thing == null || !ageStages.IsValid())
                return false;
            
            if (thingAgeStagesMap.ContainsKey(thing))
                thingAgeStagesMap.Remove(thing);

            thingAgeStagesMap[thing] = (AgeStages) ageStages.Clone();
            return true;
        }
        
        /// <summary>
        /// Get Which AgeStage one would be in at age
        /// </summary>
        /// <returns>Correct AgeStage or Adult if invalid AgeStages</returns>
        public int GetAgeStage(float age)
        {
            //AgeStage.Adult is the default and most vanilla like
            if (age < minAgeBaby)
                return Adult;

            if (age >= minAgeAdult)
                return Adult;

            var progression = GetChildhoodProgression(age);
            var total = 0;
            
            for(var i = 0; i < ageStagePercents.Length; i++)
            {
                if (total >= progression)
                    return i;
                total += ageStagePercents[i];
            }

            return Adult;
        }

        /// <summary>
        /// Percent progression of overall childhood
        /// </summary>
        public int GetChildhoodProgression(float age)
        {
            if (age <= minAgeBaby)
                return ZeroPercent;
            if (age >= minAgeAdult)
                return OneHundredPercent;
            
            return (int) (OneHundredPercent * (age - minAgeBaby) / GetChildhoodDuration());
        }
        /// <summary>
        /// Number of years childhood lasts
        /// </summary>
        public float GetChildhoodDuration()
        {
            return minAgeAdult - minAgeBaby;
        }

        public bool IsValid()
        {
            if (ageStagePercents.Length != 4)
                return false;
            var total = 0;
            foreach(var percent in ageStagePercents)
            {
                total += percent;
            }
            
            
            return (total == OneHundredPercent) && (minAgeAdult >= minAgeBaby) && (minAgeBaby >= 0);
        }
        public static int GetAgeStage(Pawn pawn)
        {
            if (pawn == null)
                return Adult;
            if (AreLifeStagesAgeStages(pawn))
                return MathTools.Clamp(pawn.ageTracker.CurLifeStageIndex, Baby, Adult);

            var ageStages = GetAgeStages(pawn);
            return ageStages == null ? Adult : ageStages.GetAgeStage(pawn.ageTracker.AgeBiologicalYears);
        }


        public static bool AreLifeStagesAgeStages(Pawn pawn)
        {
            //Can cache results if this is too slow
            List<LifeStageAge> lifeStages = pawn?.RaceProps?.lifeStageAges;
            if (lifeStages == null || lifeStages.Count < 5)
                return false;
            return (lifeStages[0].def == DefDatabase<LifeStageDef>.GetNamed("HumanlikeBaby")) &&
                   (lifeStages[1].def == DefDatabase<LifeStageDef>.GetNamed("HumanlikeToddler")) &&
                   (lifeStages[2].def == DefDatabase<LifeStageDef>.GetNamed("HumanlikeChild")) &&
                   (lifeStages[3].def == DefDatabase<LifeStageDef>.GetNamed("HumanlikeTeenager")) &&
                   (lifeStages[4].def == DefDatabase<LifeStageDef>.GetNamed("HumanlikeAdult"));

        }
        public static bool IsAgeStage(Pawn pawn, int ageStage)
        {
            return GetAgeStage(pawn) == ageStage;
        }
        public static bool IsYoungerThan(Pawn pawn, int ageStage)
        {
            return GetAgeStage(pawn) < ageStage;
        }
        public static bool IsOlderThan(Pawn pawn, int ageStage)
        {
            return !IsAgeStage(pawn, ageStage) && !IsYoungerThan(pawn, ageStage);
        }
        public object Clone()
        {
            return new AgeStages(this);
        }

    }
}