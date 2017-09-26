using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using Minalear.Engine;
using Minalear.Engine.Content;

namespace DynamicShadows
{
    public class Scene
    {
        public List<Light> Lights { get; private set; }
        public List<RenderNode> RenderNodes { get; private set; }
        public List<SpriteNode> SpriteNodes { get; private set; }

        public Scene()
        {
            Lights = new List<Light>();
            RenderNodes = new List<RenderNode>();
            SpriteNodes = new List<SpriteNode>();
        }

        public Light AttachLight(Color4 color, Vector3 position)
        {
            Light light = new Light(color, position);
            Lights.Add(light);

            return light;
        }
        public RenderNode AttachRenderNode(Model model, Vector3 position)
        {
            RenderNode node = new RenderNode(model, position);
            RenderNodes.Add(node);

            return node;
        }
        public SpriteNode AttachSpriteNode(Texture2D sprite, Vector3 position)
        {
            SpriteNode node = new SpriteNode(sprite, position);
            SpriteNodes.Add(node);

            return node;
        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < RenderNodes.Count; i++)
                RenderNodes[i].Update(gameTime);

            for (int i = 0; i < SpriteNodes.Count; i++)
                SpriteNodes[i].Update(gameTime);
        }
    }
}
