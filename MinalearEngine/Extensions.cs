using System;

namespace Minalear.Engine
{
    public static class Extensions
    {
        public static int SizeOf(this float[] array)
        {
            return array.Length * sizeof(float);
        }
        public static int SizeOf(this int[] array)
        {
            return array.Length * sizeof(int);
        }
    }
}
