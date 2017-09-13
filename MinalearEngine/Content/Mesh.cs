using System;
using OpenTK;

namespace Minalear.Engine.Content
{
    public class Mesh : IContentType
    {
        public Material Material;
        public float[] VertexData;

        public void Dispose() { }
    }
}
