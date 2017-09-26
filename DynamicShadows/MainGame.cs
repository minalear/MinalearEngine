using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DynamicShadows
{
    public class MainGame : Game
    {
        private ContentManager content;

        private RenderEngine renderEngine;
        private Scene scene;
        private Camera camera;

        private bool toggle = false;

        private float fpsTimer;
        private int frameCounter;

        public MainGame() : base("Dynamic Shadows Prototype", 1280, 720) { }

        public override void Initialize()
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            renderEngine = new RenderEngine();

            Window.KeyUp += (sender, e) =>
            {
                if (e.Key == OpenTK.Input.Key.Space)
                {
                    toggle = !toggle;
                    if (toggle)
                        GL.PolygonMode(MaterialFace.Front, PolygonMode.Line);
                    else
                        GL.PolygonMode(MaterialFace.Front, PolygonMode.Fill);
                }
            };
        }
        public override void LoadContent()
        {
            content = new ContentManager();
            content.ContentDirectory = "Content/";

            camera = new Camera(new Vector3(0.5f, 0.5f, 13f), new Vector3(0f, -MathHelper.PiOver2, 0f));

            scene = new Scene();
            RenderNode geo = scene.AttachRenderNode(content.LoadObjModel("Models/dungeon.obj"), Vector3.Zero);
            geo.Rotation = new Vector3(0f, -MathHelper.PiOver2, 0f);

            scene.AttachSpriteNode(content.LoadTexture2D("Textures/explorer.png"), new Vector3(0f, 0.6f, 7f));

            renderEngine.CompileScene(content, scene);
        }

        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            renderEngine.RenderScene(camera, gameTime);

            //FPS Counter
            frameCounter++;
            fpsTimer += gameTime.Delta;
            if (fpsTimer >= 10.0f)
            {
                Console.WriteLine("{0} fps.", frameCounter / 10);
                frameCounter = 0;
                fpsTimer = 0f;
            }

            Window.SwapBuffers();
        }

        public override void Update(GameTime gameTime)
        {
            scene.Update(gameTime);
            camera.Update(gameTime);

            if (camera.locked)
            {
                camera.Y = scene.SpriteNodes[0].Position.Y + 1f;
                camera.X = scene.SpriteNodes[0].Position.X;
            }
        }
    }
}
