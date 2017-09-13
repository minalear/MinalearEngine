using System;
using OpenTK;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class VertexLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "v"; } }

        public override void ParseLine(ModelLoader loader, string line)
        {
            string[] tokens = line.Split(new char[]{ ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float x, y, z;
            x = tokens[0].ParseFloat();
            y = tokens[1].ParseFloat();
            z = tokens[2].ParseFloat();

            loader.AddNewVertex(new Vector3(x, y, z));
        }
    }
}
