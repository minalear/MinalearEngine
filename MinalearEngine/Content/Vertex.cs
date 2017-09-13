using OpenTK;

namespace Minalear.Engine.Content
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector2 Texture;

        public Vector3 Normal;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        public float[] GetFloatArray()
        {
            return new float[] {
                Position.X, Position.Y, Position.Z,
                Texture.X, Texture.Y,
                Normal.X, Normal.Y, Normal.Z,
                Tangent.X, Tangent.Y, Tangent.Z,
                Bitangent.X, Bitangent.Y, Bitangent.Z,
            };
        }
    }
}
