using System;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Handles
{
    /// <summary>
    /// Binds a vertex array. Disposes of by using <code>0</code>.
    /// </summary>
    public readonly struct OpenVertexArray: IDisposable
    {
        /// <summary>
        /// Binds the given <paramref name="reference"/>.
        /// </summary>
        /// <param name="reference">The reference to use.</param>
        public OpenVertexArray(uint reference) => GL.BindVertexArray(reference);

        /// <summary>
        /// Binds the vertex array <code>0</code>.
        /// </summary>
        public void Dispose() => GL.BindVertexArray(0);
    }
}