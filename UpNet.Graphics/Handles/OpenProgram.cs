using System;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Handles
{
    /// <summary>
    /// Uses a program. Disposes of by using <code>0</code>.
    /// </summary>
    public readonly struct OpenProgram: IDisposable
    {
        /// <summary>
        /// Uses the given <paramref name="reference"/>.
        /// </summary>
        /// <param name="reference">The reference to use.</param>
        public OpenProgram(uint reference) => GL.UseProgram(reference);

        /// <summary>
        /// Uses the program <code>0</code>.
        /// </summary>
        public void Dispose() => GL.UseProgram(0);
    }
}