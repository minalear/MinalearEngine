using System;
using OpenTK;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class RenderNode : SceneNode
    {
        protected Model model;

        public RenderNode(Model model, Vector3 position)
            : base(position)
        {
            this.model = model;
        }

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }
    }
}
