using System;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Lang;

namespace UpNet.Graphics.Graphics.Textures
{
    /// <summary>
    /// Managed interface to GL textures.
    /// </summary>
    public abstract class ManagedTexture : Managed<uint>
    {
        /// <summary>
        /// Gets the current texture for the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">The target to get.</param>
        /// <returns>Returns the binding.</returns>
        /// <exception cref="ArgumentException">Thrown when the target is unsupported.</exception>
        public static uint Current(TextureTarget target)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Get appropriate texture for the given target.
            var texture = 0;
            if (target == TextureTarget.Texture1d)
                GL.GetInteger(GetPName.TextureBinding1d, ref texture);
            else if (target == TextureTarget.Texture2d)
                GL.GetInteger(GetPName.TextureBinding2d, ref texture);
            else if (target == TextureTarget.Texture3d)
                GL.GetInteger(GetPName.TextureBinding3d, ref texture);
            else if (target == TextureTarget.TextureRectangle)
                GL.GetInteger(GetPName.TextureBindingRectangle, ref texture);
            else if (target == TextureTarget.TextureCubeMap)
                GL.GetInteger(GetPName.TextureBindingCubeMap, ref texture);
            else if (target == TextureTarget.Texture1dArray)
                GL.GetInteger(GetPName.TextureBinding1dArray, ref texture);
            else if (target == TextureTarget.Texture2dArray)
                GL.GetInteger(GetPName.TextureBinding2dArray, ref texture);
            else if (target == TextureTarget.TextureBuffer)
                GL.GetInteger(GetPName.TextureBindingBuffer, ref texture);
            else if (target == TextureTarget.Texture2dMultisample)
                GL.GetInteger(GetPName.TextureBinding2dMultisample, ref texture);
            else if (target == TextureTarget.Texture2dMultisampleArray)
                GL.GetInteger(GetPName.TextureBinding2dMultisampleArray, ref texture);
            else
                throw new ArgumentException($"Unsupported target {target}", nameof(target));
            GLException.ThrowGlErrors("Error getting current texture binding");

            // Return that texture.
            return unchecked((uint) texture);
        }

        /// <summary>
        /// Binds no texture.
        /// </summary>
        public static void BindNone(TextureTarget target) =>
            GL.BindTexture(target, 0);

        /// <summary>
        /// The texture target to use.
        /// </summary>
        public TextureTarget Target { get; }

        /// <summary>
        /// Creates a texture and initializes it's GL reference.
        /// </summary>
        /// <param name="target">The texture target.</param>
        public ManagedTexture(TextureTarget target = TextureTarget.Texture2d)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();
            
            // Transfer values.
            Target = target;

            // Create texture object.
            Reference = GL.GenTexture();
            GLException.ThrowGlErrors("Error generating texture");
        }

        /// <summary>
        /// Gets the currently assigned texture, bind this texture.
        /// </summary>
        /// <returns>Returns the object name of the previous texture.</returns>
        protected uint SwapIn()
        {
            // Get current. Bind this if not currently assigned, then return current.
            var current = Current(Target);
            if (current != Reference)
                GL.BindTexture(Target, Reference);
            return current;
        }

        /// <summary>
        /// Sets the texture to the previous texture if it wasn't this texture.
        /// </summary>
        /// <param name="resultOfSwapIn">The previous texture, usually the result of <see cref="SwapIn"/>.</param>
        protected void SwapBackOut(uint resultOfSwapIn)
        {
            // If not the same as this, bind the previous texture.
            if (resultOfSwapIn != Reference)
                GL.BindTexture(Target, resultOfSwapIn);
        }

        /// <summary>
        /// Gets or sets the <see cref="TextureMinFilter"/>.
        /// </summary>
        public TextureMinFilter MinFilter
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Get parameter, throw errors.
                var result = 0;
                GL.GetTexParameteri(Target, GetTextureParameter.TextureMinFilter, ref result);
                GLException.ThrowGlErrors("Error getting minification filter");

                // Swap back out.
                SwapBackOut(revert);

                // Return filter.
                return (TextureMinFilter) result;
            }
            set
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Set parameter, throw errors.
                GL.TexParameteri(Target, TextureParameterName.TextureMinFilter, (int) value);
                GLException.ThrowGlErrors("Error setting minification filter");

                // Swap back out.
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextureMagFilter"/>.
        /// </summary>
        public TextureMagFilter MagFilter
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Get parameter, throw errors.
                var result = 0;
                GL.GetTexParameteri(Target, GetTextureParameter.TextureMagFilter, ref result);
                GLException.ThrowGlErrors("Error getting magnification filter");

                // Swap back out.
                SwapBackOut(revert);

                // Return filter.
                return (TextureMagFilter) result;
            }
            set
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Set parameter, throw errors.
                GL.TexParameteri(Target, TextureParameterName.TextureMagFilter, (int) value);
                GLException.ThrowGlErrors("Error setting magnification filter");

                // Swap back out.
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextureWrapMode"/> on the R coordinate.
        /// </summary>
        public TextureWrapMode WrapR
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Get parameter, throw errors.
                var result = 0;
                GL.GetTexParameteri(Target, GetTextureParameter.TextureWrapRExt, ref result);
                GLException.ThrowGlErrors("Error getting wrap mode");

                // Swap back out.
                SwapBackOut(revert);

                // Return wrap mode.
                return (TextureWrapMode) result;
            }
            set
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Set parameter, throw errors.
                GL.TexParameteri(Target, TextureParameterName.TextureWrapRExt, (int) value);
                GLException.ThrowGlErrors("Error setting wrap mode");

                // Swap back out.
                SwapBackOut(revert);
            }
        }


        /// <summary>
        /// Gets or sets the <see cref="TextureWrapMode"/> on the S coordinate.
        /// </summary>
        public TextureWrapMode WrapS
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Get parameter, throw errors.
                var result = 0;
                GL.GetTexParameteri(Target, GetTextureParameter.TextureWrapS, ref result);
                GLException.ThrowGlErrors("Error getting wrap mode");

                // Swap back out.
                SwapBackOut(revert);

                // Return wrap mode.
                return (TextureWrapMode) result;
            }
            set
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Set parameter, throw errors.
                GL.TexParameteri(Target, TextureParameterName.TextureWrapS, (int) value);
                GLException.ThrowGlErrors("Error setting wrap mode");

                // Swap back out.
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TextureWrapMode"/> on the T coordinate.
        /// </summary>
        public TextureWrapMode WrapT
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Get parameter, throw errors.
                var result = 0;
                GL.GetTexParameteri(Target, GetTextureParameter.TextureWrapT, ref result);
                GLException.ThrowGlErrors("Error getting wrap mode");

                // Swap back out.
                SwapBackOut(revert);

                // Return wrap mode.
                return (TextureWrapMode) result;
            }
            set
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for parameter operation.
                var revert = SwapIn();

                // Set parameter, throw errors.
                GL.TexParameteri(Target, TextureParameterName.TextureWrapT, (int) value);
                GLException.ThrowGlErrors("Error setting wrap mode");

                // Swap back out.
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Generates the mip-map for the texture.
        /// </summary>
        public void GenerateMipMaps()
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for mip-map operation.
            var revert = SwapIn();

            // Generate for target.
            GL.GenerateMipmap(Target);
            GLException.ThrowGlErrors("Error generating mip-maps");

            // Swap back out.
            SwapBackOut(revert);
        }

        /// <summary>
        /// Binds the texture. Does not check if there's an active texture already.
        /// </summary>
        public void Bind() =>
            GL.BindTexture(Target, Reference);

        /// <summary>
        /// Binds the texture. If <paramref name="checkActive"/> is <c>true</c>, checks if there's an active texture
        /// and throws an <see cref="InvalidOperationException"/> if there is and it's not this texture.
        /// </summary>
        /// <param name="checkActive">True if current texture should be checked for.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is another texture active.</exception>
        public void Bind(bool checkActive)
        {
            // If no check needed, just use it.
            if (!checkActive)
            {
                GL.BindTexture(Target, Reference);
                return;
            }

            // Get the current texture. If same, return, if other, throw error.
            var current = Current(Target);
            if (current == Reference)
                return;
            if (current != 0)
                throw new InvalidOperationException("Other texture currently active");

            // Use this texture.
            GL.BindTexture(Target, Reference);
        }

        /// <summary>
        /// Binds no texture to the target if this texture was active.
        /// </summary>
        public void Unbind()
        {
            if (Reference == Current(Target))
                BindNone(Target);
        }

        /// <summary>
        /// Deletes the texture.
        /// </summary>
        protected override void DisposeContent() =>
            GL.DeleteTexture(Reference);
    }
}