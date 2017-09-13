using System;
using OpenTK.Graphics;

namespace Minalear.Engine.Content
{
    public class Material : IContentType
    {
        public string Name;

        public float SpecularExponent;

        public string DiffuseMapPath;
        public string NormalMapPath;
        public string SpecularMapPath;

        public Color4 AmbientColor;
        public Color4 DiffuseColor;
        public Color4 SpecularColor;

        public Texture2D DiffuseMap, NormalMap, SpecularMap;

        public Material(string name)
        {
            Name = name;
        }

        public void Dispose() { }
        public override string ToString()
        {
            return string.Format("mtl [ {0} ]", Name);
        }
    }
}
