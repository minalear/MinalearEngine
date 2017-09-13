using System;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class UseMaterialLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "usemtl"; } }

        public override void ParseLine(ModelLoader loader, string line)
        {
            loader.SetActiveMaterial(line);
        }
    }
}
