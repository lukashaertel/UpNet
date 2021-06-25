using System;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Handles;

namespace UpNet.Graphics.Graphics
{
    /// <summary>
    /// A GL texture object.
    /// </summary>
    [Obsolete]
    public class LegacyTexture : IDisposable
    {
        /// <summary>
        /// Activates the texture unit and returns the open active texture handle.
        /// </summary>
        /// <param name="unit">The unit to use.</param>
        /// <returns>Returns the open active texture handle.</returns>
        public static OpenActiveTexture Activate(TextureUnit unit) => new(unit);

        /// <summary>
        /// The reference to the texture.
        /// </summary>
        public uint Reference { get; private set; }

        /// <summary>
        /// The target that this texture is used for.
        /// </summary>
        public TextureTarget Target { get; }

        /// <summary>
        /// Creates the vertex array. Resolves the attributes.
        /// </summary>
        public LegacyTexture(TextureTarget target = TextureTarget.Texture2d)
        {
            Target = target;

            // Generate texture.
            Reference = GL.GenTexture();
        }

        /// <summary>
        /// Uses the texture and returns the open texture handle.
        /// </summary>
        /// <returns>Returns the open texture handle.</returns>
        public OpenTexture Bind() => new(Target, Reference);

        /// <summary>
        /// True if the vertex array and the buffers were already disposed of.
        /// </summary>
        public bool IsDisposed => uint.MaxValue == Reference;

        /// <summary>
        /// Disposes of the underlying GL texture.
        /// </summary>
        /// <param name="disposing">True when called form <see cref="Dispose()"/>.</param>
        protected virtual void Dispose(bool disposing)
        {
            // Already disposed, return.
            if (IsDisposed)
                return;

            // Dispose of GL resources.
            if (disposing)
            {
                // Delete texture and set disposed.
                GL.DeleteTexture(Reference);
                Reference = uint.MaxValue;
            }
        }

        /// <summary>
        /// Disposes of the underlying GL texture.
        /// </summary>
        public void Dispose()
        {
            // Dispose from dispose and suppress finalization.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}