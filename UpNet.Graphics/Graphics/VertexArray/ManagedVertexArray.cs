using System;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Graphics.Buffers;
using UpNet.Graphics.Graphics.Shading;
using UpNet.Graphics.Lang;

namespace UpNet.Graphics.Graphics.VertexArray
{
    /// <summary>
    /// Managed interface to GL vertex arrays.
    /// </summary>
    public sealed class ManagedVertexArray : Managed<uint>
    {
        /// <summary>
        /// Gets the current vertex array.
        /// </summary>
        public static uint Current
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Get vertex array, throw errors.
                var vertexArray = 0;
                GL.GetInteger(GetPName.VertexArray, ref vertexArray);
                GLException.ThrowGlErrors("Error getting current vertex array");

                // Return vertex array.
                return unchecked((uint) vertexArray);
            }
        }

        /// <summary>
        /// Binds no vertex array.
        /// </summary>
        public static void BindNone() =>
            GL.BindVertexArray(0);

        /// <summary>
        /// Creates a vertex array and initializes it's GL reference.
        /// </summary>
        public ManagedVertexArray()
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Generate vertex array.
            Reference = GL.GenVertexArray();
            GLException.ThrowGlErrors("Error generating vertex array");
        }


        /// <summary>
        /// Gets the currently assigned vertex array, bind this vertex array.
        /// </summary>
        /// <returns>Returns the object name of the previous vertex array.</returns>
        private uint SwapIn()
        {
            // Get current. Bind this if not currently assigned, then return current.
            var current = Current;
            if (current != Reference)
                GL.BindVertexArray(Reference);
            return current;
        }

        /// <summary>
        /// Sets the vertex array to the previous vertex array if it wasn't this vertex array.
        /// </summary>
        /// <param name="resultOfSwapIn">The previous vertex array, usually the result of <see cref="SwapIn"/>.</param>
        private void SwapBackOut(uint resultOfSwapIn)
        {
            // If not the same as this, bind the previous vertex array.
            if (resultOfSwapIn != Reference)
                GL.BindVertexArray(resultOfSwapIn);
        }

        /// <summary>
        /// Checks if the given attrib is enabled.
        /// </summary>
        /// <param name="attrib">The attrib to check for</param>
        /// <returns></returns>
        public bool IsEnabled(Attrib attrib)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for check operation.
            var revert = SwapIn();

            var enabled = 0;
            GL.GetVertexAttribi(
                unchecked((uint) attrib.Location),
                VertexAttribPropertyARB.VertexAttribArrayEnabled,
                ref enabled);
            GLException.ThrowGlErrors($"Error checking if {attrib} is enabled");

            // Swap back out.
            SwapBackOut(revert);

            // Return if enabled.
            return 0 != enabled;
        }

        /// <summary>
        /// Enables the vertex attrib array for the target.
        /// </summary>
        /// <param name="attrib">The target to enable for.</param>
        public void Enable(Attrib attrib)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            var revert = SwapIn();
            GL.EnableVertexAttribArray(unchecked((uint) attrib.Location));
            GLException.ThrowGlErrors($"Error enabling {attrib.Name} for vertex array");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Disables the vertex attrib array for the target.
        /// </summary>
        /// <param name="attrib">The target to disable for.</param>
        public void Disable(Attrib attrib)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            var revert = SwapIn();
            GL.DisableVertexAttribArray(unchecked((uint) attrib.Location));
            GLException.ThrowGlErrors($"Error disabling {attrib.Name} for vertex array");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Applies the given <paramref name="layout"/> for the <paramref name="attrib"/>.
        /// </summary>
        /// <param name="attrib">The target to apply to.</param>
        /// <param name="layout">The layout to apply.</param>
        public void Apply(Attrib attrib, PointerLayout layout)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            var revert = SwapIn();
            GL.VertexAttribPointer(
                (uint) attrib.Location,
                layout.Size,
                layout.Type,
                layout.Normalized,
                layout.Stride,
                layout.Offset);
            GLException.ThrowGlErrors($"Error setting {attrib.Name} array layout");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Binds the vertex array. Does not check if there's an active vertex array already.
        /// </summary>
        public void Bind() =>
            GL.BindVertexArray(Reference);

        /// <summary>
        /// Binds the vertex array. If <paramref name="checkActive"/> is <c>true</c>, checks if there's an active vertex
        /// array and throws an <see cref="InvalidOperationException"/> if there is and it's not this vertex array.
        /// </summary>
        /// <param name="checkActive">True if current vertex array should be checked for.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is another vertex array active.</exception>
        public void Bind(bool checkActive)
        {
            // If no check needed, just use it.
            if (!checkActive)
            {
                GL.BindVertexArray(Reference);
                return;
            }

            // Get the current vertex array. If same, return, if other, throw error.
            var current = Current;
            if (current == Reference)
                return;
            if (current != 0)
                throw new InvalidOperationException("Other vertex array currently active");

            // Use this vertex array.
            GL.BindVertexArray(Reference);
        }

        /// <summary>
        /// Binds no vertex array if this vertex array was active.
        /// </summary>
        public void Unbind()
        {
            if (Reference == Current)
                BindNone();
        }

        /// <summary>
        /// Deletes the vertex array.
        /// </summary>
        protected override void DisposeContent() =>
            GL.DeleteVertexArray(Reference);
    }
}