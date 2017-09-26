using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace DynamicShadows
{
    public class Light : SceneNode
    {
        private Color4 color;
        private int shadowMap = -1;
        private int shadowResolution = -1;

        public Light(Color4 color, Vector3 position)
            : base(position)
        {
            this.color = color;
        }

        public void GenShadowMap(int resolution)
        {
            if (shadowMap != -1)
                GL.DeleteTexture(shadowMap);
            shadowResolution = resolution;

            shadowMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, shadowMap);

            for (int face = 0; face < 6; face++)
            {
                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + face, 0, PixelInternalFormat.DepthComponent,
                    resolution, resolution, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            }

            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        }

        public int ShadowMap
        {
            get { return shadowMap; }
        }
        public Color4 Color
        {
            get { return color; }
            set { color = value; }
        }
        public Vector3 ColorRGB
        {
            get { return new Vector3(color.R, color.G, color.B); }
        }
        public int Resolution
        {
            get { return shadowResolution; }
        }
    }

    public enum LightTypes
    {
        Point,
        Directional
    }
    public enum ShadowTypes
    {
        None,
        Static,
        Dynamic
    }
}
