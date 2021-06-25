using System;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using UpNet.Graphics.Graphics.Textures;

namespace UpNet.Graphics.Graphics.Loading
{
    /// <summary>
    /// Static tools for <see cref="Image{TPixel}"/>
    /// </summary>
    public static class Images
    {
        /// <summary>
        /// Loads the texture from an image.
        /// </summary>
        /// <param name="image">The image to load.</param>
        /// <typeparam name="T">The type of the pixels.</typeparam>
        /// <returns>Returns a new, initialized 2D texture.</returns>
        /// <exception cref="ArgumentException">
        /// Throws an argument exception if the format can't be determined.
        /// </exception>
        public static ManagedTexture2D Load2DFrom<T>(Image<T> image) where T : unmanaged, IPixel<T>
        {
            // Get formats and type.
            var internalFormat = image.FindInternalFormat()
                                 ?? throw new ArgumentException("Cannot find internal format for image", nameof(image));
            var pixelFormat = image.FindPixelFormat()
                              ?? throw new ArgumentException("Cannot find pixel format for image", nameof(image));
            var pixelType = image.FindPixelType()
                            ?? throw new ArgumentException("Cannot find pixel type for image", nameof(image));

            // Create new managed texture for the parameters.
            var result = new ManagedTexture2D(internalFormat, image.Width, image.Height);

            // External swap in texture.
            var revert = ManagedTexture.Current(TextureTarget.Texture2d);
            result.Bind();

            // Unsafe region, needs to work with pointers.
            unsafe
            {
                // Iterate all rows, get pointer to row data.
                for (var row = 0; row < image.Height; row++)
                    fixed (void* rowPointer = &image.GetPixelRowSpan(row).GetPinnableReference())
                        // Pass pointer to row into sub-image, reverse vertical offset to match orientations.
                        GL.TexSubImage2D(result.Target, 0, 0, (image.Height - 1) - row,
                            image.Width, 1, pixelFormat, pixelType, rowPointer);
            }

            // Generate mip-maps.
            result.GenerateMipMaps();

            // External swap back out.
            GL.BindTexture(TextureTarget.Texture2d, revert);

            // Return the result.
            return result;
        }
    }
}