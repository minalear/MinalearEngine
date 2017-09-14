using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace ShaderForge
{
    public class SceneRenderer
    {
        const int SHADOW_WIDTH = 4096, SHADOW_HEIGHT = 4096;

        private int vao;
        public int depthMapFBO, depthMapTexture;
        private ShaderProgram shader, shadow;

        private List<Model> models;
        private float timer = 0f;

        private Vector3 lightDir;
        private Vector3 sunPos;
        private Vector3 target;

        public SceneRenderer()
        {
            models = new List<Model>();
        }

        public void Draw(GameTime gameTime, Vector3 cameraPos)
        {
            /*Two render passes.  One for shadow mapping to the FBO and then the final pass*/
            Matrix4 model = Matrix4.CreateScale(1f) * Matrix4.CreateTranslation(Vector3.Zero);
            Matrix4 camera = Matrix4.LookAt(cameraPos, cameraPos + new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f));
            GL.BindVertexArray(vao);

            //Render shadow map to depth buffer
            GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Vector3 lightPos = new Vector3(35f, 40f, 30f);
            //Vector3 lightDir = new Vector3(-0.766f, -0.613f, -0.192f);

            shadow.Use();
            shadow.SetMatrix4("model", false, model);
            Matrix4 lightSpaceMatrix =
                Matrix4.LookAt(sunPos, sunPos + lightDir, new Vector3(0f, 1f, 0f)) *
                Matrix4.CreateOrthographic(150f, 150f, 1f, 250f);
            shadow.SetMatrix4("lightSpaceMatrix", false, lightSpaceMatrix);

            renderScene();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //Final render pass with shadow map
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Viewport(0, 0, 1280, 720);

            shader.Use();
            shader.SetMatrix4("view", false, camera);
            shader.SetMatrix4("model", false, model);
            shader.SetMatrix4("lightSpaceMatrix", false, lightSpaceMatrix);
            shader.SetVector3("cameraPosition", cameraPos);
            shader.SetVector3("lightDirection", lightDir);

            renderScene();

            GL.BindVertexArray(0);
        }
        public void Update(GameTime gameTime)
        {
            sunPos.X = (float)Math.Cos(timer) * 100f;
            sunPos.Y = (float)Math.Sin(timer) * 100f;

            //target.X += MathHelper.Clamp(RNG.NextFloat(-1f, 1f), -100f, 100f);
            //target.Z += MathHelper.Clamp(RNG.NextFloat(-1f, 1f), -100f, 100f);

            lightDir = target - sunPos;
            lightDir.Normalize();

            timer += gameTime.Delta * 0.1f;
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

            GL.EnableVertexAttribArray(0); //POS
            GL.EnableVertexAttribArray(1); //UV
            GL.EnableVertexAttribArray(2); //NORMAL

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            setupShader(content);
            setupFBO();
        }

        private float[] getVertexData()
        {
            List<float> vertexData = new List<float>();
            foreach (Model model in models)
                vertexData.AddRange(model.GetVertexData());
            return vertexData.ToArray();
        }
        private void setupShader(ContentManager content)
        {
            shader = content.LoadShaderProgram("Shaders/model_vert.glsl", "Shaders/model_frag.glsl");

            shader.Use();
            shader.SetMatrix4("proj", false, Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, 1280f / 720f, 1f, 100f));

            shader.SetInt("texture0", 0);
            shader.SetInt("texture1", 1);
            shader.SetInt("texture2", 2);
            shader.SetInt("shadowMap", 3);

            shadow = content.LoadShaderProgram("Shaders/shadow_vert.glsl", "Shaders/shadow_frag.glsl");
        }
        private void setupFBO()
        {
            depthMapFBO = GL.GenFramebuffer();

            //Generate Texture with Depth Components
            depthMapTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthMapTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] borderColor = { 1f, 1f, 1f, 1f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            //Bind to Framebuffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMapTexture, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        private void renderScene()
        {
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, depthMapTexture);

            int index = 0;
            for (int i = 0; i < models.Count; i++)
            {
                for (int k = 0; k < models[i].Meshes.Length; k++)
                {
                    Mesh mesh = models[i].Meshes[k];
                    int vertexCount = mesh.VertexData.Length / 8;

                    GL.ActiveTexture(TextureUnit.Texture0);
                    mesh.Material.DiffuseMap.Bind();

                    GL.ActiveTexture(TextureUnit.Texture1);
                    mesh.Material.NormalMap.Bind();

                    GL.ActiveTexture(TextureUnit.Texture2);
                    mesh.Material.SpecularMap.Bind();

                    GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                    index += vertexCount;
                }
            }
        }
    }
}
