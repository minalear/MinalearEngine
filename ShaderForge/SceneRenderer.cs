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
        private int vao;
        private ShaderProgram shader;

        private List<Model> models;

        public SceneRenderer()
        {
            models = new List<Model>();
        }

        public void Draw(GameTime gameTime, Vector3 cameraPos)
        {
            Matrix4 camera = Matrix4.LookAt(cameraPos, cameraPos + new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f));
            Matrix4 model = Matrix4.CreateScale(1f) * Matrix4.CreateTranslation(Vector3.Zero);

            shader.Use();
            shader.SetMatrix4("view", false, camera);
            shader.SetMatrix4("model", false, model);
            shader.SetVector3("cameraPosition", cameraPos);

            GL.BindVertexArray(vao);
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
            GL.BindVertexArray(0);
        }
        public void Update(GameTime gameTime)
        {

        }

        public void AttachModel(Model model)
        {
            models.Add(model);
        }
        public void CompileScene(ContentManager content)
        {
            shader = content.LoadShaderProgram("Shaders/model_vert.glsl", "Shaders/model_frag.glsl");

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            List<float> vertexData = new List<float>();
            foreach (Model model in models)
                vertexData.AddRange(model.GetVertexData());
            float[] buffer = vertexData.ToArray();

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buffer.Length * sizeof(float), buffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //POS
            GL.EnableVertexAttribArray(1); //UV
            GL.EnableVertexAttribArray(2); //NORMAL

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

            shader.Use();
            shader.SetMatrix4("model", false, Matrix4.Identity);
            shader.SetMatrix4("proj", false, Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, 1280f / 720f, 0.01f, 1000f));

            shader.SetInt("texture0", 0);
            shader.SetInt("texture1", 1);
            shader.SetInt("texture2", 2);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
    }
}
