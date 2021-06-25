using System;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Handles
{
    /// <summary>
    /// Binds a texture to <see cref="Target"/>. Disposes of by using <code>0</code>.
    /// </summary>
    public readonly struct OpenTexture: IDisposable
    {
        public TextureTarget Target { get; }

        /// <summary>
        /// Binds the given <paramref name="reference"/> to <see cref="Target"/>.
        /// </summary>
        /// <param name="target">The texture target.</param>
        /// <param name="reference">The reference to use.</param>
        public OpenTexture(TextureTarget target, uint reference)
        {
            Target = target;
            GL.BindTexture(target, reference);
        }

        /// <summary>
        /// Binds <code>0</code> to <see cref="Target"/>.
        /// </summary>
        public void Dispose() => GL.BindTexture(Target, 0);
    }
}