using System;
using System.Collections.Generic;

namespace Minalear.Engine.Content.MaterialLoader
{
    public class MaterialLibrary
    {
        public Dictionary<string, Material> Materials { get; private set; }

        public MaterialLibrary()
        {
            Materials = new Dictionary<string, Material>();
        }
    }
}
