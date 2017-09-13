using System;

namespace Minalear.Engine.Content
{
    /// <summary>
    /// Used by the ContentManager to warn of shader compilation errors.
    /// </summary>
    public class ShaderCompilationException : Exception
    {
        public ShaderCompilationException() : base() { }
        public ShaderCompilationException(string msg) : base(msg) { }
        public ShaderCompilationException(string msg, Exception inner) : base(msg, inner) { }
    }
}
