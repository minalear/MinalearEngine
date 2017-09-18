using System;
using OpenTK;
using OpenTK.Graphics;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class Light : SceneNode
    {
        protected Color4 color;
        protected int shadowMap;

        public Light(Color4 color, Vector3 position)
            : base(position)
        {
            this.color = color;
        }

        public Color4 Color
        {
            get { return color; }
            set { color = value; }
        }
        public int ShadowMap
        {
            get { return shadowMap; }
            set { shadowMap = value; }
        }
    }
}
