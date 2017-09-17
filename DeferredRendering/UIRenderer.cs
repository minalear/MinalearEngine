using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class UIRenderer
    {
        private int vao;
        private ShaderProgram shader;

        public UIRenderer(ContentManager content)
        {
            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            float[] bufferData = new float[] {
                0f, 0f, 0f, 1f, 1f, 0f,
                1f, 0f, 0f, 1f, 1f, 1f,
            };

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, bufferData.SizeOf(), bufferData, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //Pos/UV
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            shader = content.LoadShaderProgram("Shaders/ui_vert.glsl", "Shaders/ui_frag.glsl");
        }

        public void DrawTexture(int id, Vector2 position, Vector2 size)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.BindVertexArray(vao);

            Matrix4 model =
                Matrix4.CreateScale(new Vector3(size.X, size.Y, 1f)) *
                Matrix4.CreateTranslation(new Vector3(position.X, position.Y, 0f));
            Matrix4 view = Matrix4.CreateOrthographicOffCenter(0f, 1280f, 720f, 0f, -1f, 1f);

            shader.Use();
            shader.SetMatrix4("model", false, model);
            shader.SetMatrix4("view", false, view);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, id);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindVertexArray(0);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
