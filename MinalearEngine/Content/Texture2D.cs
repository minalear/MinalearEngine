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
