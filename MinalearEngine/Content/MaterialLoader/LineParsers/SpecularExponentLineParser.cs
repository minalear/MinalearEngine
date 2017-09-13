using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class SpecularExponentLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "Ns"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            float exp = line.ParseFloat();
            loader.SetSpecularExponent(exp);
        }
    }
}
