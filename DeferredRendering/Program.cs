using System;

namespace DeferredRendering
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
