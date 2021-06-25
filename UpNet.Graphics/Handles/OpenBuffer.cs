using System;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Handles
{
    /// <summary>
    /// Binds a buffer to <see cref="Target"/>. Disposes of by using <code>0</code>.
    /// </summary>
    public readonly struct OpenBuffer : IDisposable
    {
        /// <summary>
        /// The buffer target.
        /// </summary>
        public BufferTargetARB Target { get; }

        /// <summary>
        /// Binds the given <paramref name="reference"/> to <see cref="Target"/>.
        /// </summary>
        /// <param name="target">The buffer target.</param>
        /// <param name="reference">The reference to use.</param>
        public OpenBuffer(BufferTargetARB target, uint reference)
        {
            Target = target;
            GL.BindBuffer(target, reference);
        }

        /// <summary>
        /// Binds <code>0</code> to <see cref="Target"/>.
        /// </summary>
        public void Dispose() => GL.BindBuffer(Target, 0);
    }
}