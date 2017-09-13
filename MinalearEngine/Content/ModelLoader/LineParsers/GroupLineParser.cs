using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minalear.Engine.Content.ModelLoader.LineParsers
{
    internal class GroupLineParser : LineParserBase
    {
        public override string LinePrefix { get { return "g"; } }

        public override void ParseLine(ModelLoader loader, string line)
        {
            loader.AddNewGroup(new Group() { Name = line });
        }
    }
}
