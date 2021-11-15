using Verse;

namespace BabiesAndChildren
{
    public class MinAgeAdult : DefModExtension
    {
        public float minAgeAdult = 18;
    }

    public class MinAgeBaby : DefModExtension
    {
        public float minAgeBaby = 0;
    }

    public class AgeStagePercents : DefModExtension
    {
        public int[] ageStagePercents =
        {
            15,
            15,
            40,
            30
        };
    }
}