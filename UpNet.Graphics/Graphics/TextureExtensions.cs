using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace UpNet.Graphics.Graphics
{
    /// <summary>
    /// Extends <see cref="LegacyTexture"/>.
    /// </summary>
    public static class TextureExtensions
    {
        private readonly struct FormatMapping
        {
            public SizedInternalFormat InternalFormat { get; }
            public PixelFormat PixelFormat { get; }
            public PixelType PixelType { get; }

            public FormatMapping(SizedInternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType)
            {
                InternalFormat = internalFormat;
                PixelFormat = pixelFormat;
                PixelType = pixelType;
            }
        }

        // TODO Most of these are just guessed.
        /// <summary>
        /// List of what pixel format is mapped to what pixel type. 
        /// </summary>
        private static readonly Dictionary<Type, FormatMapping> FormatMappings = new()
        {
            {typeof(Rgb24), new FormatMapping(SizedInternalFormat.Rgb8, PixelFormat.Rgb, PixelType.UnsignedByte)},
            {typeof(Rgba32), new FormatMapping(SizedInternalFormat.Rgba8, PixelFormat.Rgba, PixelType.UnsignedByte)},
            //
            // {typeof(A8), PixelFormat.Alpha},
            // {typeof(Bgr24), PixelFormat.Bgr},
            // {typeof(Bgra32), PixelFormat.Bgra},
            // {typeof(Bgra4444), PixelFormat.Bgra},
            // {typeof(Bgra5551), PixelFormat.Bgra},
            // {typeof(Byte4), PixelFormat.UnsignedInt},
            // {typeof(L8), PixelFormat.Luminance},
            // {typeof(L16), PixelFormat.Luminance},
            // {typeof(La16), PixelFormat.LuminanceAlpha},
            // {typeof(La32), PixelFormat.LuminanceAlpha},
            // {typeof(NormalizedByte2), PixelFormat.Rg},
            // {typeof(NormalizedByte4), PixelFormat.Rgba},
            // {typeof(NormalizedShort2), PixelFormat.Rg},
            // {typeof(NormalizedShort4), PixelFormat.Rgba},
            // {typeof(Rg32), PixelFormat.Rg},
            // {typeof(Rgb24), PixelFormat.Rgb},
            // {typeof(Rgb48), PixelFormat.Rgb},
            // {typeof(Rgba32), PixelFormat.Rgba},
            // {typeof(Rgba64), PixelFormat.Rgba},
            // {typeof(Rgba1010102), PixelFormat.Rgba},
            // {typeof(RgbaVector), PixelFormat.Rgba},
            // {typeof(Short2), PixelFormat.Rg},
            // {typeof(Short4), PixelFormat.Rg},
        };


        /// <summary>
        /// Sets the minification filter for the <see cref="LegacyTexture.Target"/>. The <see cref="LegacyTexture.Reference"/> must
        /// be bound.
        /// </summary>
        /// <param name="texture">The target.</param>
        /// <param name="filter">The filter.</param>
        public static void SetMinFilter(this LegacyTexture texture, TextureMinFilter filter) =>
            GL.TexParameteri(texture.Target, TextureParameterName.TextureMinFilter, (int) filter);

        /// <summary>
        /// Sets the magnification filter for the <see cref="LegacyTexture.Target"/>. The <see cref="LegacyTexture.Reference"/> must
        /// be bound.
        /// </summary>
        /// <param name="texture">The target.</param>
        /// <param name="filter">The filter.</param>
        public static void SetMagFilter(this LegacyTexture texture, TextureMagFilter filter) =>
            GL.TexParameteri(texture.Target, TextureParameterName.TextureMagFilter, (int) filter);

        /// <summary>
        /// Sets the horizontal wrap for the <see cref="LegacyTexture.Target"/>. The <see cref="LegacyTexture.Reference"/> must be
        /// bound.
        /// </summary>
        /// <param name="texture">The target.</param>
        /// <param name="wrapMode">The wrap mode.</param>
        public static void SetWrapHorizontal(this LegacyTexture texture, TextureWrapMode wrapMode) =>
            GL.TexParameteri(texture.Target, TextureParameterName.TextureWrapS, (int) wrapMode);

        /// <summary>
        /// Sets the vertical wrap for the <see cref="LegacyTexture.Target"/>. The <see cref="LegacyTexture.Reference"/> must be
        /// bound.
        /// </summary>
        /// <param name="texture">The target.</param>
        /// <param name="wrapMode">The wrap mode.</param>
        public static void SetWrapVertical(this LegacyTexture texture, TextureWrapMode wrapMode) =>
            GL.TexParameteri(texture.Target, TextureParameterName.TextureWrapT, (int) wrapMode);


        /// <summary>
        /// Generates the mip maps for the <see cref="LegacyTexture.Target"/>. The <see cref="LegacyTexture.Reference"/> must be
        /// bound.
        /// </summary>
        /// <param name="texture">The target.</param>
        public static void GenerateMipMaps(this LegacyTexture texture) =>
            GL.GenerateMipmap(texture.Target);

        /// <summary>
        /// Loads the data of the image into the texture. The texture must be bound.
        /// </summary>
        /// <param name="texture">The texture that's target is used.</param>
        /// <param name="image">The image to load from.</param>
        /// <typeparam name="T">The type of the image pixels.</typeparam>
        /// <exception cref="ArgumentException">Thrown when the image pixel type has no mapping for GL.</exception>
        public static void LoadFrom<T>(this LegacyTexture texture, Image<T> image)
            where T : unmanaged, IPixel<T>
        {
            // Map format if possible.
            var pixelType = typeof(T);
            if (!FormatMappings.TryGetValue(pixelType, out var formats))
                throw new ArgumentException($"Unknown pixel type {pixelType.Name}", nameof(image));

            // Determine levels.
            var levels = (int) MathHelper.Floor(MathHelper.Log2(MathHelper.Max(image.Width, image.Height)));

            // Set storage, pass level count and image size.
            GL.TexStorage2D(texture.Target, levels, formats.InternalFormat, image.Width, image.Height);

            // Unsafe region, needs to work with pointers.
            unsafe
            {
                // Iterate all rows, get pointer to row data.
                for (var row = 0; row < image.Height; row++)
                    fixed (void* rowPointer = &image.GetPixelRowSpan(row).GetPinnableReference())
                        // Pass pointer to row into sub-image, reverse vertical offset to match orientations.
                        GL.TexSubImage2D(texture.Target, 0, 0, image.Height - row,
                            image.Width, 1, formats.PixelFormat, formats.PixelType, rowPointer);
            }
        }
    }
}