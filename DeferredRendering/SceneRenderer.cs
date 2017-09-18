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
        private Light light;

        private int vao, fbo;
        private ShaderProgram coreShader, shadowShader;

        private UIRenderer uiRenderer;
        private SkyboxRenderer skyRenderer;
        
        private float farPlane = 10f;

        private const int SHADOW_SIZE = 4096;
        private const int VERTEX_SIZE = 14;

        public SceneRenderer()
        {
            models = new List<Model>();
        }

        public void Draw(GameTime gameTime, Camera camera)
        {
            renderShadowScene(camera);
            renderCoreScene(camera);

            /*GL.DepthFunc(DepthFunction.Lequal);
            skyRenderer.RenderCubeMap(cubeMap, camera);
            GL.DepthFunc(DepthFunction.Less);*/
        }

        public void AttachModel(Model model)
        {
            models.Add(model);
        }
        public void AttachLight(Light light)
        {
            this.light = light;
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
            GL.EnableVertexAttribArray(4); //Bitangent

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 5 * sizeof(float));
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 8 * sizeof(float));
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, VERTEX_SIZE * sizeof(float), 11 * sizeof(float));

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
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.0f, 1f, farPlane);

            GL.BindVertexArray(vao);

            GL.Viewport(0, 0, SHADOW_SIZE, SHADOW_SIZE);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            shadowShader.Use();
            shadowShader.SetMatrix4("model", false, model);
            shadowShader.SetMatrix4("proj", false, proj);
            shadowShader.SetFloat("farPlane", farPlane);

            Matrix4[] views = getViewData(light.Position);
            shadowShader.SetVector3("lightPosition", light.Position);

            for (int i = 0; i < 6; i++)
            {
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.TextureCubeMapPositiveX + i, light.ShadowMap, 0);
                GL.Clear(ClearBufferMask.DepthBufferBit);
                shadowShader.SetMatrix4("view", false, views[i]);

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

            GL.BindVertexArray(0);

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, 1280, 720);
        }
        public void renderCoreScene(Camera camera)
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

            coreShader.SetVector3("lightPosition", light.Position);

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
            light.ShadowMap = shadowMap;
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
            fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
}
