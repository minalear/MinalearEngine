using System;

namespace Minalear.Engine
{
    public struct GameTime
    {
        public float Delta { get { return (float)ElapsedTime.TotalSeconds; } }
        public TimeSpan ElapsedTime { get; set; }
        public TimeSpan TotalTime { get; set; }
    }
}
