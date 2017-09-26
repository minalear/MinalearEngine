using System;
using System.Diagnostics;
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
        private Stopwatch timer = new Stopwatch();

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

            scene.AttachLight(new Color4(0.95f, 0.63f, 0.34f, 1f), Vector3.Zero).CasterType = ShadowTypes.Dynamic;
            scene.AttachLight(new Color4(0.24f, 0.06f, 0.49f, 1f), new Vector3(6f, 2f, 3f));
            scene.AttachLight(new Color4(0.1f, 0.1f, 0.1f, 1f), new Vector3(1f, 2f, 3f));
            scene.AttachLight(new Color4(0.14f, 0.37f, 0.21f, 1f), new Vector3(-3f, 3f, 2f));

            renderEngine.CompileScene(content, scene);
        }

        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderEngine.RenderScene(camera, gameTime);
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
