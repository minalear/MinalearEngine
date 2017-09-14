using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace ShaderForge
{
    public class MainGame : Game
    {
        private ContentManager content;
        private SceneRenderer sceneRenderer;
        private SpriteRenderer spriteRenderer;

        private Vector3 camera;

        public MainGame() : base("ShaderForge v2", 1280, 720) { }

        public override void Initialize()
        {
            GL.ClearColor(Color4.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.Multisample);
            //GL.Enable(EnableCap.FramebufferSrgb); //Gamma Correction

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
        }
        public override void LoadContent()
        {
            content = new ContentManager();
            content.ContentDirectory = "Content/";

            sceneRenderer = new SceneRenderer();
            sceneRenderer.AttachModel(content.LoadObjModel("Models/town.obj"));
            sceneRenderer.CompileScene(content);

            spriteRenderer = new SpriteRenderer(content);

            camera = new Vector3(-10f, 10f, 50f);
        }

        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            sceneRenderer.Draw(gameTime, camera);

            spriteRenderer.Draw(gameTime, camera);
            spriteRenderer.TestDraw(sceneRenderer.depthMapTexture);

            Window.SwapBuffers();
        }
        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Key.D))
                camera.X += 10f * gameTime.Delta;
            if (keyState.IsKeyDown(Key.A))
                camera.X -= 10f * gameTime.Delta;

            spriteRenderer.Update(gameTime);
            sceneRenderer.Update(gameTime);
        }
    }
}
