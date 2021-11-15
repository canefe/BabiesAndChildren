using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace BabiesAndChildren
{

    public static class MathTools
    {
        public static int Avg(params int[] numbers)
        {
            if (numbers.Length == 0)
            {
                return 0;
            }

            int sum = 0;
            foreach (var num in numbers)
            {
                sum += num;
            }

            return sum / numbers.Length;
        }

        /// <summary>
        /// Seed fixed Random function
        /// seed = pawn.ageTracker.AgeBiologicalTicks
        /// </summary>
        public class Fixed_Rand
        {
            private Random rand;

            private void init(long seed)
            {
                init((int) seed);
            }
            private void init(int seed)
            {
                rand = new Random(seed);
                CLog.DevMessage("   Seed = " + seed);
            }
            public Fixed_Rand(Pawn pawn)
            {
                init(pawn.ageTracker.AgeBiologicalTicks + pawn.ageTracker.AgeChronologicalTicks);
            }

            public Fixed_Rand(long seed)
            {
                init(seed);
            }
            public Fixed_Rand(int seed)
            {
                init(seed);
            }

            public bool Fixed_RandChance(double chance)
            {
                double t = rand.NextDouble();
                if (t < chance) return true;
                else return false;
            }

            public bool Fixed_RandBool()
            {
                bool t = (rand.Next(0, 2) == 1);
                return t;
            }

            public double Fixed_RandDouble(Double a, Double b)
            {
                double t = a + ((b - a) * rand.NextDouble());
                return t;
            }

            public int Fixed_RandInt(int a, int b)
            {
                int t = rand.Next(a, b);
                return t;
            }

            public float Fixed_RandFloat(float a, float b)
            {
                return (float) Fixed_RandDouble(a, b);
            }

            public object Fixed_RandElement<T>(List<T> collection)
            {
                if (collection.Count == 0)
                    return null;
                
                return collection[Fixed_RandInt(0, collection.Count - 1)];
            }
        }

        public static int Clamp(int num, int min, int max)
        {
            if (num < min)
                return min;
            if (num > max)
                return max;
            return num;
        }
    }
}