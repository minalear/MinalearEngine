using System;
using OpenTK;

namespace DynamicShadows
{
    public class SceneNode
    {
        public Vector3 Position, Rotation, Scale;

        public SceneNode(Vector3 position)
        {
            Position = position;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
        }

        public virtual void Update(Minalear.Engine.GameTime gameTime) { }
        public virtual Matrix4 GetTransform()
        {
            return
                Matrix4.CreateScale(Scale) * 
                Matrix4.CreateRotationX(Rotation.X) *
                Matrix4.CreateRotationY(Rotation.Y) *
                Matrix4.CreateRotationZ(Rotation.Z) *
                Matrix4.CreateTranslation(Position);
        }
    }
}
