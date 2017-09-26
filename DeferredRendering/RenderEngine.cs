using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public sealed class RenderEngine : IDisposable
    {
        private Scene scene;

        private int sceneVAO, sceneVBO;
        private int quadVAO, quadVBO;
        private int gBuffer, gPosition, gNormal, gAlbedoSpec;
        private int rboDepth;

        private float timer = 0.0f;

        private ShaderProgram gBufferShader, quadShader;

        public int GPosition { get { return gPosition; } }
        public int GNormal { get { return gNormal; } }
        public int GAlbedoSpec { get { return gAlbedoSpec; } }
        
        public RenderEngine() { }

        public void ConstructScene(ContentManager content, Scene scene)
        {
            this.scene = scene;

            initBufferData(scene);
            initGraphicsBuffer(1280, 720);
            initShaders(content);
        }
        public void RenderScene(GameTime gameTime, Camera camera)
        {
            timer += gameTime.Delta;
            timer = (timer >= 10f) ? 0f : timer;

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, gBuffer);
            GL.ClearColor(Color4.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            gBufferShader.Use();
            gBufferShader.SetMatrix4("view", false, camera.View);
            gBufferShader.SetMatrix4("proj", false, camera.Projection);

            //1. - Render scene to the graphics buffer
            GL.BindVertexArray(sceneVAO);

            int index = 0, vertexCount = 0;
            for (int i = 0; i < scene.RenderNodes.Count; i++)
            {
                Matrix4 transform = Matrix4.CreateRotationY(scene.RenderNodes[i].Rotation.Y);
                gBufferShader.SetMatrix4("model", false, transform);

                for (int m = 0; m < scene.RenderNodes[i].Model.Meshes.Length; m++)
                {
                    Mesh mesh = scene.RenderNodes[i].Model.Meshes[m];
                    vertexCount = mesh.VertexData.Length / 8;

                    GL.ActiveTexture(TextureUnit.Texture0);
                    mesh.Material.DiffuseMap.Bind();

                    GL.ActiveTexture(TextureUnit.Texture1);
                    mesh.Material.SpecularMap.Bind();

                    GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                    index += vertexCount;
                }
            }
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //2. - Final render pass
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(quadVAO);

            GL.ClearColor(Color4.SkyBlue);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            quadShader.Use();
            quadShader.SetVector3("cameraPosition", camera.Position);
            quadShader.SetVector3("lightPosition", camera.Position);
            quadShader.SetVector3("lightColor", new Vector3(1f, 1f, 1f));

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, gPosition);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, gNormal);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoSpec);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }

        public void Dispose() { }

        private void initBufferData(Scene scene)
        {
            //Scene VAO and VBO
            float[] bufferData = getSceneBufferData(scene);

            sceneVAO = GL.GenVertexArray();
            GL.BindVertexArray(sceneVAO);

            sceneVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, sceneVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.SizeOf(), bufferData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //Pos
            GL.EnableVertexAttribArray(1); //UV
            GL.EnableVertexAttribArray(2); //Normal

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0 * sizeof(float)); //Pos (XYZ)
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float)); //UV (XY)
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float)); //Normal (XYZ)
            //GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 11 * sizeof(float), 8 * sizeof(float)); //Tangent (XYZ)

            //Quad VAO and VBO
            float[] quadBuffer = new float[] {
                0f, 0f, 0f, 1f, 1f, 0f,
                1f, 0f, 0f, 1f, 1f, 1f,
            };

            quadVAO = GL.GenVertexArray();
            GL.BindVertexArray(quadVAO);

            quadVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, quadVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, quadBuffer.SizeOf(), quadBuffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        private void initGraphicsBuffer(int width, int height)
        {
            gBuffer = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, gBuffer);

            //Position Color Buffer
            gPosition = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gPosition);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, width, height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, gPosition, 0);

            //Normal Color Buffer
            gNormal = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gNormal);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb16f, width, height, 0, PixelFormat.Rgb, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, gNormal, 0);

            //Albedo + Specular Color Buffer
            gAlbedoSpec = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, gAlbedoSpec);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, TextureTarget.Texture2D, gAlbedoSpec, 0);

            DrawBuffersEnum[] attachments = new DrawBuffersEnum[3] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1, DrawBuffersEnum.ColorAttachment2 };
            GL.DrawBuffers(3, attachments);

            //Depth buffer
            rboDepth = GL.GenRenderbuffer();
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, rboDepth);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, rboDepth);

            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new InvalidOperationException("Framebuffer Error");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        private void initShaders(ContentManager content)
        {
            gBufferShader = content.LoadShaderProgram("Shaders/gBufferVert.glsl", "Shaders/gBufferFrag.glsl");

            gBufferShader.Use();
            gBufferShader.SetMatrix4("model", false, Matrix4.Identity);
            gBufferShader.SetMatrix4("view", false, Matrix4.Identity);
            gBufferShader.SetMatrix4("proj", false, Matrix4.Identity);

            gBufferShader.SetInt("diffuseMap", 0);
            gBufferShader.SetInt("specularMap", 1);

            quadShader = content.LoadShaderProgram("Shaders/lightingVert.glsl", "Shaders/lightingFrag.glsl");

            quadShader.Use();
            quadShader.SetMatrix4("model", false, Matrix4.CreateScale(1280f, 720f, 1f));
            quadShader.SetMatrix4("view", false, Matrix4.CreateOrthographicOffCenter(0f, 1280f, 720f, 0f, -1f, 1f));

            quadShader.SetInt("gPosition", 0);
            quadShader.SetInt("gNormal", 1);
            quadShader.SetInt("gAlbedoSpec", 2);
        }

        private float[] getSceneBufferData(Scene scene)
        {
            List<float> bufferData = new List<float>();

            foreach (RenderNode node in scene.RenderNodes)
            {
                bufferData.AddRange(node.Model.GetVertexData());
            }

            return bufferData.ToArray();
        }
    }
}
