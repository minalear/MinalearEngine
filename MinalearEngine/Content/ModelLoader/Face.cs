using System;

namespace Minalear.Engine.Content.ModelLoader
{
    public struct Face
    {
        public Vertex[] Vertices;

        public override string ToString()
        {
            return string.Format("f ( {0}/{1}/{2} )", Vertices[0], Vertices[1], Vertices[2]);
        }
    }
}
