using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DynamicShadows
{
    public class RenderEngine
    {
        private int sceneVAO, sceneVBO;
        private int spriteVAO;
        private int shadowFBO;

        private ShaderProgram sceneShader, spriteShader, shadowShader, dirShader;
        private Scene scene;

        private float farPlane = 30f;
        private Light dirLight;

        const int VERTEX_LENGTH = 8;
        const int SHADOW_MAP_RESOLUTION = 1024;

        public RenderEngine() { }

        public void CompileScene(ContentManager content, Scene scene)
        {
            this.scene = scene;

            initVertexData(scene);
            initShaders(content);
            initFBO();
            initLights();

            //Pre-render all shadow maps
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                renderShadowMaps(scene.Lights[i]);
            }

            dirLight = new Light(Color4.White, Vector3.Zero);
            dirLight.Type = LightTypes.Directional;
            dirLight.GenShadowMap(4096);

            renderDirectionalShadowMap(dirLight);
        }
        public void RenderScene(Camera camera, GameTime gameTime)
        {
            Matrix4 model;
            scene.Lights[0].Position = scene.SpriteNodes[0].Position + new Vector3(0.5f, 0.5f, 0f); //Debug

            for (int i = 0; i < scene.Lights.Count; i++)
            {
                if (scene.Lights[i].CasterType == ShadowTypes.Dynamic)
                    renderShadowMaps(scene.Lights[i]);
            }

            sceneShader.Use();
            sceneShader.SetMatrix4("view", false, camera.View);
            sceneShader.SetMatrix4("proj", false, camera.Projection);

            sceneShader.SetFloat("farPlane", farPlane);
            sceneShader.SetVector3("cameraPosition", camera.Position);

            sceneShader.SetVector3("directLight.direction", new Vector3(-1f, -1f, 1f));
            sceneShader.SetVector3("directLight.color", Vector3.One);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, dirLight.ShadowMap);

            for (int i = 0; i < scene.Lights.Count; i++)
            {
                Light light = scene.Lights[i];
                sceneShader.SetVector3(string.Format("lights[{0}].position", i), light.Position);
                sceneShader.SetVector3(string.Format("lights[{0}].color", i), light.ColorRGB);

                GL.ActiveTexture(TextureUnit.Texture3 + i);
                GL.BindTexture(TextureTarget.TextureCubeMap, light.ShadowMap);
            }

            GL.BindVertexArray(sceneVAO);

            int index = 0, vertexCount;
            for (int i = 0; i < scene.RenderNodes.Count; i++)
            {
                RenderNode node = scene.RenderNodes[i];
                model = node.GetTransform();
                sceneShader.SetMatrix4("model", false, model);

                for (int k = 0; k < node.Model.Meshes.Length; k++)
                {
                    Mesh mesh = node.Model.Meshes[k];
                    vertexCount = mesh.VertexData.Length / VERTEX_LENGTH;

                    GL.ActiveTexture(TextureUnit.Texture0);
                    mesh.Material.DiffuseMap.Bind();

                    GL.ActiveTexture(TextureUnit.Texture1);
                    mesh.Material.SpecularMap.Bind();

                    GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                    index += vertexCount;
                }
            }

            //Bind VAO
            GL.BindVertexArray(spriteVAO);

            //Set shader variables
            spriteShader.Use();
            spriteShader.SetMatrix4("view", false, camera.View);
            spriteShader.SetMatrix4("proj", false, camera.Projection);

            float spriteWidth = 1f / 7;
            float spriteHeight = 1f;

            spriteShader.SetFloat("spriteWidth", spriteWidth);
            spriteShader.SetFloat("spriteHeight", spriteHeight);

            //Loop through each sprite and render it
            for (int i = 0; i < scene.SpriteNodes.Count; i++)
            {
                SpriteNode node = scene.SpriteNodes[i];

                //Sprite specific shader variables
                model = node.GetTransform();
                spriteShader.SetMatrix4("model", false, model);
                spriteShader.SetInt("X", 0);

                //Bind sprite texture
                GL.ActiveTexture(TextureUnit.Texture0);
                node.Sprite.Bind();
                //GL.BindTexture(TextureTarget.Texture2D, dirLight.ShadowMap); //DEBUG

                //Draw vertices
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            }
            
            //Unbind VAO and disable Blend
            GL.BindVertexArray(0);
        }

        private void renderDirectionalShadowMap(Light light)
        {
            Matrix4 proj = Matrix4.CreateOrthographic(20f, 20f, 1f, 25f);

            GL.BindVertexArray(sceneVAO);
            GL.Viewport(0, 0, light.Resolution, light.Resolution);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);

            Vector3 sunPos = new Vector3(8f, 10f, -4f);
            Matrix4 view = Matrix4.LookAt(sunPos, sunPos + new Vector3(-1f, -1f, 1f), new Vector3(0f, 1f, 0f));

            Matrix4 LightSpaceMatrix = view * proj;
            sceneShader.Use();
            sceneShader.SetMatrix4("lightSpaceMatrix", false, LightSpaceMatrix);

            dirShader.Use();
            dirShader.SetMatrix4("proj", false, proj);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, light.ShadowMap, 0);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            dirShader.SetMatrix4("view", false, view);

            int index = 0, vertexCount;
            for (int k = 0; k < scene.RenderNodes.Count; k++)
            {
                RenderNode node = scene.RenderNodes[k];
                Matrix4 model = node.GetTransform();
                dirShader.SetMatrix4("model", false, model);

                for (int j = 0; j < node.Model.Meshes.Length; j++)
                {
                    Mesh mesh = node.Model.Meshes[j];
                    vertexCount = mesh.VertexData.Length / VERTEX_LENGTH;

                    GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                    index += vertexCount;
                }
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, 1280, 720);
            GL.BindVertexArray(0);
        }
        private void renderShadowMaps(Light light)
        {
            Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1f, 0.1f, farPlane);

            GL.BindVertexArray(sceneVAO);
            GL.Viewport(0, 0, light.Resolution, light.Resolution);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
            
            shadowShader.Use();
            shadowShader.SetMatrix4("proj", false, proj);
            shadowShader.SetFloat("farPlane", farPlane);
            shadowShader.SetVector3("lightPosition", light.Position);

            Matrix4[] faceTransforms = getFaceTransforms(light.Position);
            for (int i = 0; i < 6; i++)
            {
                TextureTarget face = TextureTarget.TextureCubeMapPositiveX + i;
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, face, light.ShadowMap, 0);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                shadowShader.SetMatrix4("view", false, faceTransforms[i]);

                int index = 0, vertexCount;
                for (int k = 0; k < scene.RenderNodes.Count; k++)
                {
                    RenderNode node = scene.RenderNodes[k];
                    Matrix4 model = node.GetTransform();
                    shadowShader.SetMatrix4("model", false, model);

                    for (int j = 0; j < node.Model.Meshes.Length; j++)
                    {
                        Mesh mesh = node.Model.Meshes[j];
                        vertexCount = mesh.VertexData.Length / VERTEX_LENGTH;

                        GL.DrawArrays(PrimitiveType.Triangles, index, vertexCount);
                        index += vertexCount;
                    }
                }
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, 1280, 720);
            GL.BindVertexArray(0);
        }

        private void initVertexData(Scene scene)
        {
            /* SCENE SETUP */
            float[] bufferData = fetchSceneVertexData(scene);
            
            sceneVAO = GL.GenVertexArray();
            GL.BindVertexArray(sceneVAO);

            sceneVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, sceneVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.SizeOf(), bufferData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //Position
            GL.EnableVertexAttribArray(1); //TexCoords
            GL.EnableVertexAttribArray(2); //Normal

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VERTEX_LENGTH * sizeof(float), 0 * sizeof(float));
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VERTEX_LENGTH * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VERTEX_LENGTH * sizeof(float), 5 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            /*SPRITE SETUP*/
            float[] spriteBufferData = new float[] {
               -0.5f, 0.5f, 0f,     0f, 0f,
               -0.5f,-0.5f, 0f,     0f, 1f,
                0.5f, 0.5f, 0f,     1f, 0f,

                0.5f, 0.5f, 0f,     1f, 0f,
               -0.5f,-0.5f, 0f,     0f, 1f,
                0.5f,-0.5f, 0f,     1f, 1f,
            };

            spriteVAO = GL.GenVertexArray();
            GL.BindVertexArray(spriteVAO);

            int spriteVBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, spriteVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, spriteBufferData.SizeOf(), spriteBufferData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //Position
            GL.EnableVertexAttribArray(1); //TexCoords

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        private void initShaders(ContentManager content)
        {
            sceneShader = content.LoadShaderProgram("Shaders/scene_vert.glsl", "Shaders/scene_frag.glsl");

            sceneShader.Use();
            sceneShader.SetMatrix4("model", false, Matrix4.Identity);
            sceneShader.SetMatrix4("view", false, Matrix4.Identity);
            sceneShader.SetMatrix4("proj", false, Matrix4.Identity);

            sceneShader.SetInt("NumLights", scene.Lights.Count);

            sceneShader.SetInt("diffuseMap", 0);
            sceneShader.SetInt("specularMap", 1);
            sceneShader.SetInt("directLight.shadowMap", 2);

            int startIndex = 3;
            for (int i = 0; i < scene.Lights.Count; i++)
            {
                sceneShader.SetInt(string.Format("lights[{0}].shadowMap", i), i + startIndex);
            }

            spriteShader = content.LoadShaderProgram("Shaders/sprite_vert.glsl", "Shaders/sprite_frag.glsl");

            spriteShader.Use();
            spriteShader.SetMatrix4("model", false, Matrix4.Identity);
            spriteShader.SetMatrix4("view", false, Matrix4.Identity);
            spriteShader.SetMatrix4("proj", false, Matrix4.Identity);

            spriteShader.SetInt("sprite", 0);
            spriteShader.SetFloat("offset", 0f);

            shadowShader = content.LoadShaderProgram("Shaders/shadow_vert.glsl", "Shaders/shadow_frag.glsl");

            shadowShader.Use();
            shadowShader.SetMatrix4("model", false, Matrix4.Identity);
            shadowShader.SetMatrix4("view", false, Matrix4.Identity);
            shadowShader.SetMatrix4("proj", false, Matrix4.Identity);

            shadowShader.SetVector3("lightPosition", Vector3.Zero);
            shadowShader.SetFloat("farPlane", farPlane);

            dirShader = content.LoadShaderProgram("Shaders/shadow_vert.glsl", "Shaders/dir_shadow_frag.glsl");

            dirShader.Use();
            dirShader.SetMatrix4("model", false, Matrix4.Identity);
            dirShader.SetMatrix4("view", false, Matrix4.Identity);
            dirShader.SetMatrix4("proj", false, Matrix4.Identity);
        }
        private void initFBO()
        {
            shadowFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, shadowFBO);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
        private void initLights()
        {
            for (int i = 0; i < scene.Lights.Count; i++)
                scene.Lights[i].GenShadowMap(SHADOW_MAP_RESOLUTION);
        }

        private float[] fetchSceneVertexData(Scene scene)
        {
            List<float> buffer = new List<float>();

            foreach (RenderNode node in scene.RenderNodes)
            {
                buffer.AddRange(node.Model.GetVertexData());
            }

            return buffer.ToArray();
        }
        private Matrix4[] getFaceTransforms(Vector3 position)
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
    }
}
