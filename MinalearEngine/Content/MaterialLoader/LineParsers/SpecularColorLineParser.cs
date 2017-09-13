using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class SpecularColorLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "Ks"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            string[] components = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float r, g, b;
            r = components[0].ParseFloat();
            g = components[1].ParseFloat();
            b = components[2].ParseFloat();

            loader.SetSpecularColor(r, g, b);
        }
    }
}
