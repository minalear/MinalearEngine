using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class DiffuseMapLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "map_Kd"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            loader.SetDiffuseMap(line);
        }
    }
}
