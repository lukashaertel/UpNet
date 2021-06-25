using System;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UpNet.Graphics.Graphics.Loading
{
    /// <summary>
    /// Extends <see cref="Image"/>.
    /// </summary>
    public static class ImageExtensions
    {
        /// <summary>
        /// Returns the pixel type that is generically assigned to the image.
        /// </summary>
        /// <param name="image">The image to find the pixel type for.</param>
        /// <returns>Returns the pixel type.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the actual instance of the image is not a generic image with pixel type information.
        /// </exception>
        public static Type GetPixelType(this Image image)
        {
            var imageType = image.GetType();
            if (imageType.GetGenericTypeDefinition() != typeof(Image<>))
                throw new ArgumentException($"Image does not generically specify format", nameof(image));

            return imageType.GetGenericArguments().Single();
        }

        /// <summary>
        /// Gets an appropriate sized internal format for the given image.
        /// </summary>
        /// <param name="image">The image to find the format for.</param>
        /// <returns>Returns an appropriate mapping or <c>null</c>.</returns>
        public static SizedInternalFormat? FindInternalFormat(this Image image)
        {
            // Get pixel type.
            var pixelType = image.GetPixelType();

            // Map pixel type if possible.
            if (typeof(Rgb24) == pixelType)
                return SizedInternalFormat.Rgb8;
            if (typeof(Rgba32) == pixelType)
                return SizedInternalFormat.Rgba8;
            return null;
        }

        /// <summary>
        /// Gets an appropriate pixel format for the given image.
        /// </summary>
        /// <param name="image">The image to find the format for.</param>
        /// <returns>Returns an appropriate mapping or <c>null</c>.</returns>
        public static PixelFormat? FindPixelFormat(this Image image)
        {
            // Get pixel type.
            var pixelType = image.GetPixelType();

            // Map pixel type if possible.
            if (typeof(Rgb24) == pixelType)
                return PixelFormat.Rgb;
            if (typeof(Rgba32) == pixelType)
                return PixelFormat.Rgba;
            return null;
        }

        /// <summary>
        /// Gets an appropriate pixel type for the given image.
        /// </summary>
        /// <param name="image">The image to find the format for.</param>
        /// <returns>Returns an appropriate mapping or <c>null</c>.</returns>
        public static PixelType? FindPixelType(this Image image)
        {
            // TODO.
            return PixelType.UnsignedByte;
        }
    }
}