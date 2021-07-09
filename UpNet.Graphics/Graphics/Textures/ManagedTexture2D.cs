using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using UpNet.Graphics.Lang;

namespace UpNet.Graphics.Graphics.Textures
{
    /// <summary>
    /// Managed interface to GL textures, variant for 2D.
    /// </summary>
    public sealed class ManagedTexture2D : ManagedTexture
    {
        /// <summary>
        /// The internal format that is used for the texture storage.
        /// </summary>
        public SizedInternalFormat InternalFormat { get; }

        /// <summary>
        /// The width of the texture.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the texture.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// The level count of the texture.
        /// </summary>
        public int Levels { get; }

        /// <summary>
        /// Creates and allocates the texture with the given parameters.
        /// </summary>
        /// <param name="internalFormat">The internal format to use.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="levels">The number of levels.</param>
        public ManagedTexture2D(SizedInternalFormat internalFormat, int width, int height, int levels)
            : base(TextureTarget.Texture2d)
        {
            // Transfer the values.
            InternalFormat = internalFormat;
            Width = width;
            Height = height;
            Levels = levels;

            // Set the storage.
            GL.TexStorage2D(Target, levels, internalFormat, width, height);
        }

        /// <summary>
        /// Creates and allocates the texture with the given parameters. Determines the levels from the given
        /// dimensions.
        /// </summary>
        /// <param name="internalFormat">The internal format to use.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        public ManagedTexture2D(SizedInternalFormat internalFormat, int width, int height)
            : base(TextureTarget.Texture2d)
        {
            // Determine levels.
            var levels = (int) MathHelper.Floor(MathHelper.Log2(MathHelper.Max(width, height)));

            // Transfer the values.
            InternalFormat = internalFormat;
            Width = width;
            Height = height;
            Levels = levels;

            // Swap in for texture storage operation.
            var revert = SwapIn();

            // Set the storage.
            GL.TexStorage2D(Target, levels, internalFormat, width, height);
            GLException.ThrowGlErrors("Error setting texture storage");

            // Swap back out.
            SwapBackOut(revert);
        }

        /// <summary>
        /// Returns the size composed of <see cref="Width"/> and <see cref="Height"/>.
        /// </summary>
        public Vector2i Size => new(Width, Height);

        /// <summary>
        /// Writes the image data from the given source.
        /// </summary>
        /// <param name="data">The source to write from.</param>
        /// <param name="format">The format to write in.</param>
        /// <param name="type">The type to write in.</param>
        /// <param name="level">The level to write, defaults to primary level.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void WriteData<T>(Span<T> data, PixelFormat format, PixelType type, int level = 0) where T : unmanaged
        {
            unsafe
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                var revert = SwapIn();
                fixed (void* pinned = &data.GetPinnableReference())
                    GL.TexSubImage2D(Target, level, 0, 0, Width, Height, format, type, pinned);
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Writes the image data from the given source.
        /// </summary>
        /// <param name="data">The source to write from.</param>
        /// <param name="format">The format to write in.</param>
        /// <param name="type">The type to write in.</param>
        /// <param name="level">The level to write, defaults to primary level.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void WriteData<T>(T[] data, PixelFormat format, PixelType type, int level = 0) where T : unmanaged
        {
            unsafe
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                var revert = SwapIn();
                fixed (void* pinned = &data[0])
                    GL.TexSubImage2D(Target, level, 0, 0, Width, Height, format, type, pinned);
                GLException.ThrowGlErrors("Error writing texture data");
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Writes a slice of the image data.
        /// </summary>
        /// <param name="data">The data source to write form.</param>
        /// <param name="format">The format to write in.</param>
        /// <param name="type">The type to write in.</param>
        /// <param name="xOffset">The target X location.</param>
        /// <param name="yOffset">The target Y location.</param>
        /// <param name="width">The width of the slice data.</param>
        /// <param name="height">The height of the slice data.</param>
        /// <param name="level">The level to write, defaults to primary level.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void WriteDataSlice<T>(Span<T> data, PixelFormat format, PixelType type,
            int xOffset, int yOffset, int width, int height, int level = 0) where T : unmanaged
        {
            unsafe
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                var revert = SwapIn();
                fixed (void* pinned = &data.GetPinnableReference())
                    GL.TexSubImage2D(Target, level, xOffset, yOffset, width, height, format, type, pinned);
                GLException.ThrowGlErrors("Error writing texture data");
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Writes a slice of the image data.
        /// </summary>
        /// <param name="data">The data source to write form.</param>
        /// <param name="format">The format to write in.</param>
        /// <param name="type">The type to write in.</param>
        /// <param name="xOffset">The target X location.</param>
        /// <param name="yOffset">The target Y location.</param>
        /// <param name="width">The width of the slice data.</param>
        /// <param name="height">The height of the slice data.</param>
        /// <param name="level">The level to write, defaults to primary level.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void WriteDataSlice<T>(T[] data, PixelFormat format, PixelType type,
            int xOffset, int yOffset, int width, int height, int level = 0) where T : unmanaged
        {
            unsafe
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                var revert = SwapIn();
                fixed (void* pinned = &data[0])
                    GL.TexSubImage2D(Target, level, xOffset, yOffset, width, height, format, type, pinned);
                GLException.ThrowGlErrors("Error writing texture data");
                SwapBackOut(revert);
            }
        }
    }
}