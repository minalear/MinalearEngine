using System;
using OpenTK;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class NormalLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "vn"; } }

        public override void ParseLine(ModelLoader loader, string line)
        {
            string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float x, y, z;
            x = tokens[0].ParseFloat();
            y = tokens[1].ParseFloat();
            z = tokens[2].ParseFloat();

            loader.AddNewNormal(new Vector3(x, y, z));
        }
    }
}
