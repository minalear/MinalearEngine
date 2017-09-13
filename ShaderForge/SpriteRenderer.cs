using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace ShaderForge
{
    public class SpriteRenderer
    {
        private int vao;
        private ShaderProgram shader;

        private Texture2D sprite;

        private float timer = 0.0f;
        private int index = 0;

        public SpriteRenderer(ContentManager content)
        {
            sprite = content.LoadTexture2D("Textures/sample_sprite.png");
            shader = content.LoadShaderProgram("Shaders/sprite_vert.glsl", "Shaders/sprite_frag.glsl");

            float size = 16f / 32f;
            float[] buffer = new float[] {
            //   X   Y   Z   uv  X   Y
                0f, 1f, 0f,     0f, 0f,
                0f, 0f, 0f,     0f, 1f,
                1f, 1f, 0f,     size, 0f,

                1f, 1f, 0f,     size, 0f,
                0f, 0f, 0f,     0f, 1f,
                1f, 0f, 0f,     size, 1f,
            };

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buffer.Length * sizeof(float), buffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0); //POS
            GL.EnableVertexAttribArray(1); //UV

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            shader.Use();
            shader.SetMatrix4("proj", false, Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, 1280f / 720f, 0.01f, 1000f));

            shader.SetInt("sprite", 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }

        public void Draw(GameTime gameTime, Vector3 cameraPos)
        {
            Matrix4 camera = Matrix4.LookAt(cameraPos, cameraPos + new Vector3(0f, 0f, -1f), new Vector3(0f, 1f, 0f));
            Matrix4 model =
                Matrix4.CreateScale(new Vector3(4f, 4f, 1f)) *
                Matrix4.CreateTranslation(new Vector3(cameraPos.X, 1f, 27f));

            shader.Use();
            shader.SetMatrix4("view", false, camera);
            shader.SetMatrix4("model", false, model);
            float xOffset = (index == 0) ? 0f : 0.5f;
            shader.SetVector2("uvOffset", new Vector2(xOffset, 0f));

            GL.ActiveTexture(TextureUnit.Texture0);
            sprite.Bind();

            GL.BindVertexArray(vao);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            GL.BindVertexArray(0);
        }
        public void Update(GameTime gameTime)
        {
            timer += gameTime.Delta;
            if (timer >= 0.35f)
            {
                index = (index == 1) ? 0 : 1;
                timer = 0f;
            }
        }
    }
}
