using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class SpecularMapLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "map_Ks"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            loader.SetSpecularMap(line);
        }
    }
}
