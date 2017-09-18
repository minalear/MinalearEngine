using System;
using OpenTK;
using Minalear.Engine.Content;

namespace DeferredRendering
{
    public class SceneNode
    {
        protected Vector3 position, rotation, scale;

        public SceneNode(Vector3 position)
        {
            this.position = position;

            this.rotation = Vector3.Zero;
            this.scale = Vector3.One;
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        public Vector3 Rotation
        {
            get { return rotation; }
            set { rotation = value; }
        }
        public Vector3 Scale
        {
            get { return scale; }
            set { scale = value; }
        }
    }
}
