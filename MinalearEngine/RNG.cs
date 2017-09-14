using System;

namespace Minalear.Engine
{
    public static class RNG
    {
        private static Random randomGenerator = new Random(Guid.NewGuid().GetHashCode());

        /// <summary>
        /// Returns a random integer number.
        /// </summary>
        public static int Next()
        {
            return randomGenerator.Next();
        }

        /// <summary>
        /// Returns a random integer number with an upper limit.
        /// </summary>
        public static int Next(int max)
        {
            return randomGenerator.Next(max);
        }

        /// <summary>
        /// Returns a random integer number between two values.
        /// </summary>
        public static int Next(int min, int max)
        {
            return randomGenerator.Next(min, max);
        }

        /// <summary>
        /// Returns a random double number.
        /// </summary>
        public static double NextDouble()
        {
            return randomGenerator.NextDouble();
        }

        /// <summary>
        /// Returns a random double number between two values.
        /// </summary>
        public static double NextDouble(double min, double max)
        {
            return NextDouble() * (max - min) + min;
        }

        /// <summary>
        /// Returns a random float number.
        /// </summary>
        public static float NextFloat()
        {
            return (float)randomGenerator.NextDouble();
        }

        /// <summary>
        /// Returns a random float number between two values.
        /// </summary>
        public static float NextFloat(float min, float max)
        {
            return NextFloat() * (max - min) + min;
        }
    }
}
