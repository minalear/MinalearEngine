using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class SceneRenderer
    {
        public Vector3 LightPosition;
        
        private List<Model> models;

        private int vao, fbo;
        private ShaderProgram coreShader, shadowShader;
        private int[] renderTargets;
        private Matrix4[] views;

        private UIRenderer uiRenderer;
        private const int SHADOW_SIZE = 1024;

        private int view = 3;

        public SceneRenderer()
        {
            models = new List<Model>();
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            views = new Matrix4[] {
                Matrix4.LookAt(LightPosition, LightPosition + new Vector3(1f, 0f, 0f), new Vector3(0f, 1f, 0f)),
                Matrix4.LookAt(LightPosition, LightPosition + new Vector3(-1f, 0f, 0f), new Vector3(0f, 1f, 0f)),
                Matrix4.LookAt(LightPosition, LightPosition + new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f)),
                Matrix4.LookAt(LightPosition, LightPosition + new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, -1f)),
                Matrix4.LookAt(LightPosition, LightPosition + new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 0f)),
                Matrix4.LookAt(LightPosition, LightPosition + new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f)),
            };

            renderShadowScene(camera);
            renderCoreScene(camera);

            uiRenderer.DrawTexture(renderTargets[view], new Vector2(10f, 10f), new Vector2(128f));
            uiRenderer.DrawTexture(renderTargets[view + 1], new Vector2(10f, 148f), new Vector2(128f));
        }

        public void AttachModel(Model model)
        {
            models.Add(model);
        }
        public void CompileScene(ContentManager content)
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            float[] buffer = getVertexData();

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buffer.Length * sizeof(float), buffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //Pos
            GL.EnableVertexAttribArray(1); //UV
            GL.EnableVertexAttribArray(2); //Normal

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            setupShader(content);
            setupFBO();

            uiRenderer = new UIRenderer(content);
        }

        public void renderShadowScene(Camera camera)
        {
            GL.Viewport(0, 0, SHADOW_SIZE, SHADOW_SIZE);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            
            GL.BindVertexArray(vao);

            Matrix4 model = Matrix4.CreateRotationY(MathHelper.PiOver2);
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 25f);

            shadowShader.Use();
            shadowShader.SetMatrix4("model", false, model);
            shadowShader.SetMatrix4("proj", false, proj);

            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, renderTargets[i], 0);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                shadowShader.SetMatrix4("view", false, views[i]);

                int index = 0;
                foreach (Model sceneObj in models)
                {
                    foreach (Mesh mesh in sceneObj.Meshes)
                    {
                        int vertexCount = mesh.VertexData.Length / 8;
                        GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                        index += vertexCount;
                    }
                }
            }

            GL.BindVertexArray(0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, 1280, 720);
        }
        public void renderCoreScene(Camera camera)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindVertexArray(vao);

            Matrix4 model = Matrix4.CreateRotationY(MathHelper.PiOver2);
            Matrix4 lightSpace = views[view] * Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 1f, 25f);

            coreShader.Use();
            coreShader.SetMatrix4("model", false, model);
            coreShader.SetMatrix4("view", false, camera.View);
            coreShader.SetMatrix4("proj", false, camera.Projection);

            coreShader.SetVector3("lightPosition", LightPosition);
            coreShader.SetVector3("cameraPosition", camera.Position);
            coreShader.SetMatrix4("lightSpaceMatrix", false, lightSpace);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, renderTargets[view]);

            int index = 0;
            foreach (Model sceneObj in models)
            {
                foreach (Mesh mesh in sceneObj.Meshes)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    mesh.Material.DiffuseMap.Bind();

                    int vertexCount = mesh.VertexData.Length / 8;
                    GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                    index += vertexCount;
                }
            }

            GL.BindVertexArray(0);
        }

        private float[] getVertexData()
        {
            List<float> buffer = new List<float>();
            foreach (Model model in models)
                buffer.AddRange(model.GetVertexData());
            return buffer.ToArray();
        }
        private void setupShader(ContentManager content)
        {
            coreShader = content.LoadShaderProgram("Shaders/model_vert.glsl", "Shaders/model_frag.glsl");

            coreShader.Use();
            coreShader.SetMatrix4("model", false, Matrix4.Identity);
            coreShader.SetMatrix4("view", false, Matrix4.Identity);
            coreShader.SetMatrix4("proj", false, Matrix4.Identity);

            coreShader.SetInt("diffuseMap", 0);
            coreShader.SetInt("shadowMap", 1);

            shadowShader = content.LoadShaderProgram("Shaders/shadow_vert.glsl", "Shaders/shadow_frag.glsl");
        }
        private void setupFBO()
        {
            fbo = GL.GenFramebuffer();

            renderTargets = new int[6];
            for (int i = 0; i < renderTargets.Length; i++)
            {
                renderTargets[i] = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, renderTargets[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_SIZE, SHADOW_SIZE, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            //GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, renderTarget, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
