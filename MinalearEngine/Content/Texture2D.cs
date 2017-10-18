using System;
using System.Drawing;
using Imaging = System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;

namespace Minalear.Engine.Content
{
    public class Texture2D : IContentType
    {
        private int textureID;
        private int width, height;

        public Texture2D(int id, int w, int h)
        {
            textureID = id;
            width = w;
            height = h;
        }
        public Texture2D(Bitmap bitmap)
        {
            width = bitmap.Width;
            height = bitmap.Height;

            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
            IntPtr pixelData = bitmapData.Scan0;

            textureID = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, pixelData);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //float[] borderColor = new float[] { 0f, 0f, 0f, 0f };
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);

            bitmap.UnlockBits(bitmapData);
        }

        public void Bind()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureID);
        }
        public void Dispose()
        {
            GL.DeleteTexture(textureID);
        }

        public int ID { get { return textureID; } }
        public int Width { get { return width; } }
        public int Height { get { return height; } }
    }
}
