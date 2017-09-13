using System;
using System.Collections.Generic;

namespace Minalear.Engine.Content
{
    public class Model : IContentType
    {
        public Mesh[] Meshes;

        public float[] GetVertexData()
        {
            List<float> vertexData = new List<float>();
            for (int i = 0; i < Meshes.Length; i++)
            {
                vertexData.AddRange(Meshes[i].VertexData);
            }
            return vertexData.ToArray();
        }
        public void Dispose() { }
    }
}
