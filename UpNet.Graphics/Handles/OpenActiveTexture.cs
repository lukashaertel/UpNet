using System;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Handles
{
    /// <summary>
    /// Activates a texture unit. Resets to the texture active before.
    /// </summary>
    public readonly struct OpenActiveTexture : IDisposable
    {
        /// <summary>
        /// The 
        /// </summary>
        public TextureUnit LastActive { get; }

        /// <summary>
        /// Activates the <paramref name="unit"/>. Memorizes the currently active target.
        /// </summary>
        /// <param name="unit">The unit to activate.</param>
        public OpenActiveTexture(TextureUnit unit)
        {
            // Get last active texture.
            var get = 0;
            GL.GetInteger(GetPName.ActiveTexture, ref get);
            LastActive = (TextureUnit) get;

            // Activate target.
            GL.ActiveTexture(unit);
        }

        /// <summary>
        /// Sets active texture to <see cref="LastActive"/>.
        /// </summary>
        public void Dispose() => GL.ActiveTexture(LastActive);
    }
}