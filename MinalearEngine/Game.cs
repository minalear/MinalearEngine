using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine.Content;

namespace Minalear.Engine
{
    public class Game : IDisposable
    {
        private GameTime gameTime;
        private GameWindow window;

        public Game(string title, int width, int height)
        {
            window = new GameWindow(width, height, new GraphicsMode(32, 24, 8, 4), title, GameWindowFlags.Default);
            window.RenderFrame += (sender, e) => renderFrame(e);
            window.UpdateFrame += (sender, e) => updateFrame(e);
            window.Resize += (sender, e) => Resize();

            gameTime = new GameTime();

            Initialize();
            LoadContent();
        }

        public virtual void Initialize() { }
        public virtual void LoadContent() { }
        public virtual void Resize() { }

        public virtual void Draw(GameTime gameTime) { }
        public virtual void Update(GameTime gameTime) { }

        public virtual void Run()
        {
            window.Run();
        }
        public virtual void Stop()
        {
            window.Exit();
        }
        public virtual void Dispose()
        {
            window.Dispose();
        }

        private void renderFrame(FrameEventArgs e)
        {
            Draw(gameTime);
        }
        private void updateFrame(FrameEventArgs e)
        {
            gameTime.ElapsedTime = TimeSpan.FromSeconds(e.Time);
            gameTime.TotalTime.Add(TimeSpan.FromSeconds(e.Time));

            if (Window.Focused)
                Update(gameTime);
        }

        //Properties
        public GameWindow Window { get { return window; } }
    }
}
