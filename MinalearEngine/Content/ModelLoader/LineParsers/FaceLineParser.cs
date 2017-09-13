using System;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class FaceLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "f"; } }
        public const int INDEX_OFFSET = -1; //OBJ indices are 1-based

        public override void ParseLine(ModelLoader loader, string line)
        {
            string[] faceTokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            Vertex[] vertices = new Vertex[faceTokens.Length];

            for (int i = 0; i < faceTokens.Length; i++)
            {
                string[] tokens = faceTokens[i].Split('/');

                vertices[i].VertexIndex = tokens[0].ParseInt() + INDEX_OFFSET;
                vertices[i].TextureIndex = tokens[1].ParseInt() + INDEX_OFFSET;
                vertices[i].NormalIndex = tokens[2].ParseInt() + INDEX_OFFSET;
            }

            loader.AddNewFace(new Face() { Vertices = vertices });
        }
    }
}
