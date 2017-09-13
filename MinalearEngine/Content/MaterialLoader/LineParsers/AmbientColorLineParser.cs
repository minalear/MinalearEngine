using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class AmbientColorLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "Ka"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            string[] components = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float r, g, b;
            r = components[0].ParseFloat();
            g = components[1].ParseFloat();
            b = components[2].ParseFloat();

            loader.SetAmbientColor(r, g, b);
        }
    }
}
