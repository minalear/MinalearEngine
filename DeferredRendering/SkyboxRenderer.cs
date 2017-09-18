using System;
using System.IO;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class SkyboxRenderer
    {
        private int vao;
        private int cubeID;
        private ShaderProgram shader;

        public SkyboxRenderer(ContentManager content)
        {
            shader = content.LoadShaderProgram("Shaders/skybox_vert.glsl", "Shaders/skybox_frag.glsl");

            shader.Use();
            shader.SetMatrix4("view", false, Matrix4.Identity);
            shader.SetMatrix4("proj", false, Matrix4.Identity);
            shader.SetInt("texture0", 0);

            float[] buffer = getVertexData();

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buffer.SizeOf(), buffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
        }
        public SkyboxRenderer(ContentManager content, params string[] paths)
        {
            shader = content.LoadShaderProgram("Shaders/skybox_vert.glsl", "Shaders/skybox_frag.glsl");

            shader.Use();
            shader.SetMatrix4("view", false, Matrix4.Identity);
            shader.SetMatrix4("proj", false, Matrix4.Identity);
            shader.SetInt("texture0", 0);

            float[] buffer = getVertexData();

            vao = GL.GenVertexArray();
            GL.BindVertexArray(vao);

            int vbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, buffer.SizeOf(), buffer, BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            //Create Cube
            cubeID = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeID);

            for (int i = 0; i < 6; i++)
            {
                string path = paths[i];
                using (Bitmap bitmap = new Bitmap(path))
                {
                    var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    IntPtr pixelData = data.Scan0;
                    TextureTarget target = TextureTarget.TextureCubeMapPositiveX + i;
                    GL.TexImage2D(target, 0, PixelInternalFormat.Rgba, bitmap.Width,
                        bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixelData);

                    bitmap.UnlockBits(data);
                }
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public void RenderCubeMap(int id, Camera camera)
        {
            GL.BindVertexArray(vao);

            shader.Use();
            Matrix4 view = camera.View.ClearTranslation();
            shader.SetMatrix4("view", false, view);
            shader.SetMatrix4("proj", false, camera.Projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, id);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.BindVertexArray(0);
        }
        public void RenderCubeMap(Camera camera)
        {
            GL.BindVertexArray(vao);

            shader.Use();
            Matrix4 view = camera.View.ClearTranslation();
            shader.SetMatrix4("view", false, view);
            shader.SetMatrix4("proj", false, camera.Projection);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeID);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
            GL.BindVertexArray(0);
        }

        private float[] getVertexData()
        {
            return new float[] {
                // positions          
                -1.0f,  1.0f, -1.0f,
                -1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f, -1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,

                -1.0f, -1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f,
                -1.0f, -1.0f,  1.0f,

                -1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f, -1.0f,
                 1.0f,  1.0f,  1.0f,
                 1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f,  1.0f,
                -1.0f,  1.0f, -1.0f,

                -1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f, -1.0f,
                 1.0f, -1.0f, -1.0f,
                -1.0f, -1.0f,  1.0f,
                 1.0f, -1.0f,  1.0f
            };
        }
    }
}
