using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace DeferredRendering
{
    public class Scene
    {
        public List<Camera> Cameras { get; private set; }
        public List<SceneNode> SceneObjects { get; private set; }
    }
}
