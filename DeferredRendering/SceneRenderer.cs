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
        private List<Model> models;
        public List<Light> lights;

        private int vao, shadowFBO, renderFBO;
        private ShaderProgram coreShader, shadowShader;

        private UIRenderer uiRenderer;
        private SkyboxRenderer skyRenderer;
        
        private float farPlane = 20f;

        private const int SHADOW_SIZE = 4096;
        private const int VERTEX_SIZE = 11;

        public SceneRenderer()
        {
            models = new List<Model>();
            lights = new List<Light>();
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            renderShadowScene(camera);
            renderLights(camera);

            GL.ClearColor(Color4.SkyBlue);
            //renderBlankScene(camera);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                uiRenderer.DrawTexture(light.RenderMap, new Vector2(0, 0), new Vector2(1280, 720));
            }
            GL.Disable(EnableCap.Blend);
        }

        public void AttachModel(Model model)
        {
            models.Add(model);
        }
        public void AttachLight(Light light)
        {
            lights.Add(light);
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
            GL.EnableVertexAttribArray(3); //Tangent

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 5 * sizeof(float));
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 8 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            setupShader(content);
            setupFBO();
            setupLights();

            uiRenderer = new UIRenderer(content);
            skyRenderer = new SkyboxRenderer(content);
        }

        public void renderShadowScene(Camera camera)
        {
            Matrix4 model = Matrix4.CreateRotationY(MathHelper.PiOver2);
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 0.1f, farPlane);

            GL.BindVertexArray(vao);

            GL.Viewport(0, 0, SHADOW_SIZE, SHADOW_SIZE);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);

            shadowShader.Use();
            shadowShader.SetMatrix4("model", false, model);
            shadowShader.SetMatrix4("proj", false, proj);
            shadowShader.SetFloat("farPlane", farPlane);

            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                Matrix4[] views = getViewData(light.Position);
                shadowShader.SetVector3("lightPosition", light.Position);

                for (int face = 0; face < 6; face++)
                {
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.TextureCubeMapPositiveX + face, light.ShadowMap, 0);
                    GL.Clear(ClearBufferMask.DepthBufferBit);
                    shadowShader.SetMatrix4("view", false, views[face]);

                    int index = 0;
                    foreach (Model sceneObj in models)
                    {
                        foreach (Mesh mesh in sceneObj.Meshes)
                        {
                            int vertexCount = mesh.VertexData.Length / VERTEX_SIZE;
                            GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                            index += vertexCount;
                        }
                    }
                }
            }

            GL.BindVertexArray(0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, 1280, 720);
        }
        public void renderLights(Camera camera)
        {
            GL.BindVertexArray(vao);

            Matrix4 model = Matrix4.CreateRotationY(MathHelper.PiOver2);

            coreShader.Use();
            coreShader.SetMatrix4("model", false, model);
            coreShader.SetMatrix4("view", false, camera.View);
            coreShader.SetMatrix4("proj", false, camera.Projection);

            coreShader.SetVector3("cameraPosition", camera.Position);
            coreShader.SetFloat("farPlane", farPlane);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderFBO);
            for (int i = 0; i < lights.Count; i++)
            {
                Light light = lights[i];
                
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, light.RenderMap, 0);
                GL.ClearColor(System.Drawing.Color.Transparent);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
    
                coreShader.SetVector3("lightColor", light.Color.RGB());
                coreShader.SetVector3("lightPosition", light.Position);
                coreShader.SetFloat("ambientMod", 0f);

                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.TextureCubeMap, light.ShadowMap);

                int index = 0;
                foreach (Model sceneObj in models)
                {
                    foreach (Mesh mesh in sceneObj.Meshes)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        mesh.Material.DiffuseMap.Bind();

                        GL.ActiveTexture(TextureUnit.Texture1);
                        mesh.Material.NormalMap.Bind();

                        GL.ActiveTexture(TextureUnit.Texture2);
                        mesh.Material.SpecularMap.Bind();

                        int vertexCount = mesh.VertexData.Length / VERTEX_SIZE;
                        GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                        index += vertexCount;
                    }
                }
            }
            GL.Disable(EnableCap.Blend);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            GL.BindVertexArray(0);
        }
        public void renderBlankScene(Camera camera)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BindVertexArray(vao);

            Matrix4 model = Matrix4.CreateRotationY(MathHelper.PiOver2);

            coreShader.Use();
            coreShader.SetMatrix4("model", false, model);
            coreShader.SetMatrix4("view", false, camera.View);
            coreShader.SetMatrix4("proj", false, camera.Projection);

            coreShader.SetVector3("cameraPosition", camera.Position);
            coreShader.SetFloat("farPlane", farPlane);

            coreShader.SetVector3("lightColor", Vector3.Zero);
            coreShader.SetVector3("lightPosition", Vector3.Zero);
            coreShader.SetFloat("ambientMod", 0.1f);

            //GL.ActiveTexture(TextureUnit.Texture3);
            //GL.BindTexture(TextureTarget.TextureCubeMap, light.ShadowMap);

            int index = 0;
            foreach (Model sceneObj in models)
            {
                foreach (Mesh mesh in sceneObj.Meshes)
                {
                    GL.ActiveTexture(TextureUnit.Texture0);
                    mesh.Material.DiffuseMap.Bind();

                    GL.ActiveTexture(TextureUnit.Texture1);
                    mesh.Material.NormalMap.Bind();

                    GL.ActiveTexture(TextureUnit.Texture2);
                    mesh.Material.SpecularMap.Bind();

                    int vertexCount = mesh.VertexData.Length / VERTEX_SIZE;
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
        private Matrix4[] getViewData(Vector3 position)
        {
            return new Matrix4[] {
                Matrix4.LookAt(position, position + new Vector3(1f, 0f, 0f), new Vector3(0f, -1f, 0f)),
                Matrix4.LookAt(position, position + new Vector3(-1f, 0f, 0f), new Vector3(0f, -1f, 0f)),
                Matrix4.LookAt(position, position + new Vector3(0f, 1f, 0f), new Vector3(0f, 0f, 1f)),
                Matrix4.LookAt(position, position + new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, -1f)),
                Matrix4.LookAt(position, position + new Vector3(0f, 0f, 1f), new Vector3(0f, -1f, 0f)),
                Matrix4.LookAt(position, position + new Vector3(0f, 0f, -1f), new Vector3(0f, -1f, 0f)),
            };
        }

        private void setupLights()
        {
            for (int i = 0; i < lights.Count; i++)
            {
                int shadowMap = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureCubeMap, shadowMap);

                for (int face = 0; face < 6; face++)
                {
                    GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + face, 0, PixelInternalFormat.DepthComponent,
                        SHADOW_SIZE, SHADOW_SIZE, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                }

                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

                GL.BindTexture(TextureTarget.TextureCubeMap, 0);

                //Render Texture
                int renderTarget = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, renderTarget);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, 1280, 720, 0, PixelFormat.Bgra, PixelType.Float, IntPtr.Zero);

                //Filters
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

                lights[i].ShadowMap = shadowMap;
                lights[i].RenderMap = renderTarget;
            }
        }
        private void setupShader(ContentManager content)
        {
            coreShader = content.LoadShaderProgram("Shaders/model_vert.glsl", "Shaders/model_frag.glsl");

            coreShader.Use();
            coreShader.SetMatrix4("model", false, Matrix4.Identity);
            coreShader.SetMatrix4("view", false, Matrix4.Identity);
            coreShader.SetMatrix4("proj", false, Matrix4.Identity);

            coreShader.SetInt("diffuseMap", 0);
            coreShader.SetInt("normalMap", 1);
            coreShader.SetInt("specularMap", 2);
            coreShader.SetInt("shadowMap", 3);

            shadowShader = content.LoadShaderProgram("Shaders/shadow_vert.glsl", "Shaders/shadow_frag.glsl");
        }
        private void setupFBO()
        {
            //Shadow FBO
            shadowFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //Render FBO
            renderFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, renderFBO);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
    }
}
