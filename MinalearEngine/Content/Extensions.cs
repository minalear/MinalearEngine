using System;
using System.Globalization;
using OpenTK;

namespace Minalear.Engine.Content
{
    public static class StringExtensions
    {
        public static float ParseFloat(this string floatStr)
        {
            return float.Parse(floatStr);
        }
        public static int ParseInt(this string intStr)
        {
            return int.Parse(intStr, CultureInfo.InvariantCulture.NumberFormat);
        }
    }

    public static class VectorExtensions
    {
        public static float[] ToArray(this Vector2 v)
        {
            return new float[] { v.X, v.Y };
        }
        public static float[] ToArray(this Vector3 v)
        {
            return new float[] { v.X, v.Y, v.Z };
        }
        public static float[] ToArray(this Vector4 v)
        {
            return new float[] { v.X, v.Y, v.Z, v.W };
        }
    }
}
