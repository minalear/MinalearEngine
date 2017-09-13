using System;

namespace Minalear.Engine.Content.MaterialLoader.LineParsers
{
    internal abstract class LineParserBase
    {
        public abstract string LinePrefix { get; }

        public bool CanParseLine(string prefix)
        {
            return LinePrefix.Equals(prefix, StringComparison.OrdinalIgnoreCase);
        }
        public abstract void ParseLine(MaterialLoader loader, string line);
    }
}
