using System;

namespace Minalear.Engine
{
    public abstract class SceneObject : IRenderable, IUpdateable
    {
        public abstract void Draw(GameTime gameTime);
        public abstract void Update(GameTime gameTime);
    }
}
