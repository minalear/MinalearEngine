using System;
using Minalear.Engine;

namespace ShaderForge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using (MainGame game = new MainGame())
            {
                game.Run();
            }
        }
    }
}
