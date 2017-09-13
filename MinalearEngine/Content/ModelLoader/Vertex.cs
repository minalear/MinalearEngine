using System;
using OpenTK;

namespace Minalear.Engine.Content.ModelLoader
{
    public struct Vertex
    {
        public int VertexIndex;
        public int TextureIndex;
        public int NormalIndex;

        public override string ToString()
        {
            return string.Format("v ( {0}, {1}, {2} )", VertexIndex, TextureIndex, NormalIndex);
        }
    }
}
