using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class MainGame : Game
    {
        private ContentManager content;
        
        private Camera camera;
        private SceneRenderer scene;

        public MainGame() : base("Deffered Rendering Testing", 1280, 720) { }

        public override void Initialize()
        {
            GL.ClearColor(Color4.LightSkyBlue);
            GL.Enable(EnableCap.DepthTest);

            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            
            camera = new Camera(new Vector3(0f, 10f, -16f), new Vector3(-0.588f, 0.009f, 0f));
        }
        public override void LoadContent()
        {
            content = new ContentManager();
            content.ContentDirectory = "Content/";
            
            scene = new SceneRenderer();
            scene.AttachModel(content.LoadObjModel("Models/dungeon.obj"));
            scene.AttachLight(new Light(Color4.White, new Vector3(-6f, 2f, -3f)));
            scene.CompileScene(content);
        }

        public override void Draw(GameTime gameTime)
        {
            scene.Draw(gameTime, camera);
            Window.SwapBuffers();
        }
        public override void Update(GameTime gameTime)
        {
            camera.Update(gameTime);
        }
    }
}
