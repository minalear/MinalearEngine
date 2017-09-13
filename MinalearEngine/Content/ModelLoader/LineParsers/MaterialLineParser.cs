using System;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class MaterialLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "mtllib"; } }

        public override void ParseLine(ModelLoader loader, string line)
        {
            loader.SetMaterialLibrary(line);
        }
    }
}
