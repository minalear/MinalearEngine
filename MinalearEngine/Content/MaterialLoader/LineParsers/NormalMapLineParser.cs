using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class NormalMapLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "map_Bump"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            loader.SetNormalMap(line);
        }
    }
}
