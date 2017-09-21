using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class Scene
    {
        public List<Camera> Cameras { get; private set; }
        public List<RenderNode> RenderNodes { get; private set; }
        public List<Light> Lights { get; private set; }

        public Scene()
        {
            Cameras = new List<Camera>();
            RenderNodes = new List<RenderNode>();
            Lights = new List<Light>();
        }

        public void AttachModel(Model model, Vector3 position, Vector3 rotation)
        {
            RenderNode node = new RenderNode(model, position);
            node.Rotation = rotation;

            RenderNodes.Add(node);
        }
        public void AttachLight(Color4 color, Vector3 position)
        {
            Lights.Add(new Light(color, position));
        }
    }
}
