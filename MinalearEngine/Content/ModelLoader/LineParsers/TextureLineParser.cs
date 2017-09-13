using System;
using OpenTK;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class TextureLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "vt"; } }

        public override void ParseLine(ModelLoader loader, string line)
        {
            string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            float x, y;
            x = tokens[0].ParseFloat();
            y = tokens[1].ParseFloat();

            loader.AddNewTextureCoord(new Vector2(x, y));
        }
    }
}
