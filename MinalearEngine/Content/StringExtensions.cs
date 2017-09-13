using System;
using System.Globalization;

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
}
