using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace ShaderForge
{
    public class MainGame : Game
    {
        private ContentManager content;

        private int vao, vertexCount;
        private ShaderProgram shader;

        private Model testModel;

        public MainGame() : base("ShaderForge v2", 1280, 720)
        {

        }

        public override void Initialize()
        {
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
        }
        public override void LoadContent()
        {
            content = new ContentManager();
            content.ContentDirectory = "Content/";

            testModel = content.LoadObjModel("Models/inn.obj");
            shader = content.LoadShaderProgram("Shaders/model_vert.glsl", "Shaders/model_frag.glsl");

            vao = GL.GenVertexArray();
            int vbo = GL.GenBuffer();

            float[] bufferData = testModel.GetVertexData();
            vertexCount = bufferData.Length / 8;

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.Length * sizeof(float), bufferData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //Position
            GL.EnableVertexAttribArray(1); //TexCoord
            GL.EnableVertexAttribArray(2); //Normal

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));

            shader.Use();
            shader.SetMatrix4("model", false, Matrix4.Identity);
            shader.SetMatrix4("proj", false, Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, (float)Window.Width / Window.Height, 0.01f, 1000f));
            shader.SetMatrix4("view", false, Matrix4.LookAt(new Vector3(0f, 10f, 50f), new Vector3(0f, 10f, 0f), new Vector3(0f, 1f, 0f)));

            shader.SetInt("texture0", 0);
            shader.SetInt("texture1", 1);
            shader.SetInt("texture2", 2);
        }

        public override void Draw(GameTime gameTime)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            shader.Use();
            GL.BindVertexArray(vao);

            int index = 0;
            for (int i = 0; i < testModel.Meshes.Length; i++)
            {
                Mesh mesh = testModel.Meshes[i];
                GL.ActiveTexture(TextureUnit.Texture0);
                mesh.Material.DiffuseMap.Bind();

                GL.ActiveTexture(TextureUnit.Texture1);
                mesh.Material.NormalMap.Bind();

                GL.ActiveTexture(TextureUnit.Texture2);
                mesh.Material.SpecularMap.Bind();

                GL.DrawArrays(PrimitiveType.Triangles, index, mesh.VertexData.Length / 8);

                index += mesh.VertexData.Length / 8;
            }
            
            GL.BindVertexArray(0);

            Window.SwapBuffers();
        }
        public override void Update(GameTime gameTime)
        {
            
        }
    }
}
