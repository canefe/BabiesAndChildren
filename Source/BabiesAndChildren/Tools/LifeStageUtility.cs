using BabiesAndChildren.api;
using Verse;

namespace BabiesAndChildren.Tools
{
    public static class LifeStageUtility
    {

        public static LifeStageAge GetLifeStageAgeContaining(ThingDef thing, string name)
        {
            if (thing?.race?.lifeStageAges == null)
                return null;
            
            name = name.ToLower();
            foreach (var lifeStage in thing.race.lifeStageAges)
            {
                if (lifeStage.def.defName.ToLower().Contains(name))
                {
                    return lifeStage;
                }
            }
            return null;
        }
        public static LifeStageAge GetLifeStageAge(Pawn pawn, int ageStage)
        {
            return GetLifeStageAge(pawn.def.race, ageStage);
        }

        public static LifeStageAge GetPreviousLifeStageAge(Pawn pawn)
        {
            return GetLifeStageAge(pawn, AgeStages.GetAgeStage(pawn) - 1);
        }

        public static LifeStageAge GetCurrentLifeStageAge(Pawn pawn)
        {
            return GetLifeStageAge(pawn, AgeStages.GetAgeStage(pawn));
        }

        public static LifeStageAge GetNextLifeStageAge(Pawn pawn)
        {
            return GetLifeStageAge(pawn, AgeStages.GetAgeStage(pawn) + 1);
        }

        public static LifeStageAge GetLifeStageAge(RaceProperties raceProps, int ageStage)
        {
            return raceProps.lifeStageAges[ageStage];
        }
    }
}