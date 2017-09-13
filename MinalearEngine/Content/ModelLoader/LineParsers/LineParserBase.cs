using System;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal abstract class LineParserBase
    {
        public abstract string LinePrefix { get; }

        public bool CanParseLine(string prefix)
        {
            return LinePrefix.Equals(prefix, StringComparison.OrdinalIgnoreCase);
        }
        public abstract void ParseLine(ModelLoader loader, string line);
    }
}
