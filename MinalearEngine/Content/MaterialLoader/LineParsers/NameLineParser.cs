using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal class NameLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "newmtl"; } }

        public override void ParseLine(MaterialLoader loader, string line)
        {
            loader.CreateNewMaterial(line);
        }
    }
}
