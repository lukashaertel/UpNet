using System;
using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Lang;

namespace UpNet.Graphics.Graphics.Buffers
{
    /// <summary>
    /// Managed interface to GL buffers.
    /// </summary>
    public sealed class ManagedBuffer : Managed<uint>
    {
        /// <summary>
        /// Gets the current buffer for the <paramref name="target"/>.
        /// </summary>
        /// <param name="target">
        /// The target to get, supports <see cref="BufferTargetARB.ArrayBuffer"/>
        /// and <see cref="BufferTargetARB.ElementArrayBuffer"/>.</param>
        /// <returns>Returns the binding.</returns>
        /// <exception cref="ArgumentException">Thrown when the target is unsupported.</exception>
        public static uint Current(BufferTargetARB target)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Get appropriate buffer for the given target.
            var buffer = 0;
            if (target == BufferTargetARB.ArrayBuffer)
                GL.GetInteger(GetPName.ArrayBufferBinding, ref buffer);
            else if (target == BufferTargetARB.ElementArrayBuffer)
                GL.GetInteger(GetPName.ElementArrayBufferBinding, ref buffer);
            else
                throw new ArgumentException($"Unsupported target {target}", nameof(target));
            GLException.ThrowGlErrors("Error getting current buffer binding");

            // Return that buffer.
            return unchecked((uint) buffer);
        }

        /// <summary>
        /// Binds no buffer.
        /// </summary>
        public static void BindNone(BufferTargetARB target) =>
            GL.BindBuffer(target, 0);

        /// <summary>
        /// The buffer target.
        /// </summary>
        public BufferTargetARB Target { get; }

        /// <summary>
        /// The usage of the buffer.
        /// </summary>
        public BufferUsageARB Usage { get; }

        /// <summary>
        /// Creates a buffer and initializes it's GL reference.
        /// </summary>
        /// <param name="target">The buffer target.</param>
        /// <param name="usage">The buffer usage.</param>
        public ManagedBuffer(
            BufferTargetARB target = BufferTargetARB.ArrayBuffer,
            BufferUsageARB usage = BufferUsageARB.StaticDraw)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Transfer values.
            Target = target;
            Usage = usage;

            // Create buffer object, throw errors.
            Reference = GL.GenBuffer();
            GLException.ThrowGlErrors("Error generating buffer");
        }

        /// <summary>
        /// Gets the currently assigned buffer, bind this buffer.
        /// </summary>
        /// <returns>Returns the object name of the previous buffer.</returns>
        private uint SwapIn()
        {
            // Get current. Bind this if not currently assigned, then return current.
            var current = Current(Target);
            if (current != Reference)
                GL.BindBuffer(Target, Reference);
            return current;
        }

        /// <summary>
        /// Sets the buffer to the previous buffer if it wasn't this buffer.
        /// </summary>
        /// <param name="resultOfSwapIn">The previous buffer, usually the result of <see cref="SwapIn"/>.</param>
        private void SwapBackOut(uint resultOfSwapIn)
        {
            // If not the same as this, bind the previous buffer.
            if (resultOfSwapIn != Reference)
                GL.BindBuffer(Target, resultOfSwapIn);
        }

        /// <summary>
        /// Gets the buffer's size or resizes the buffer.
        /// </summary>
        public int SizeInBytes
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for size operation.
                var revert = SwapIn();

                // Get size, throw errors.
                var result = 0;
                GL.GetBufferParameteri(Target, BufferPNameARB.BufferSize, ref result);
                GLException.ThrowGlErrors("Error getting buffer size");

                // Swap back out.
                SwapBackOut(revert);

                // Return size.
                return result;
            }
            set
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Swap buffer in for size operation.
                var revert = SwapIn();

                // Set buffer data with null-pointer and size.
                GL.BufferData(Target, value, IntPtr.Zero, Usage);
                GLException.ThrowGlErrors("Error setting buffer size");

                // Swap back out.
                SwapBackOut(revert);
            }
        }

        /// <summary>
        /// Writes the data. Resizes the buffer automatically.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void WriteData<T>(Span<T> data) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, set data, then swap back out.
            var revert = SwapIn();
            GL.BufferData(Target, data, Usage);
            GLException.ThrowGlErrors("Error writing data");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Writes the data. Resizes the buffer automatically.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void WriteData<T>(T[] data) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, set data, then swap back out.
            var revert = SwapIn();
            GL.BufferData(Target, data, Usage);
            GLException.ThrowGlErrors("Error writing data");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Writes a slice of data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="offsetInBytes">The offset at which to write.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <remarks>Ensure that the <see cref="SizeInBytes"/> is set appropriately before writing a slice.</remarks>
        public void WriteDataSlice<T>(Span<T> data, int offsetInBytes) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, set data slice, then swap back out.
            var revert = SwapIn();
            GL.BufferSubData(Target, (IntPtr) offsetInBytes, data);
            GLException.ThrowGlErrors("Error writing data slice");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Writes a slice of data.
        /// </summary>
        /// <param name="data">The data to write.</param>
        /// <param name="offsetInBytes">The offset at which to write.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <remarks>Ensure that the <see cref="SizeInBytes"/> is set appropriately before writing a slice.</remarks>
        public void WriteDataSlice<T>(T[] data, int offsetInBytes) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, set data slice, then swap back out.
            var revert = SwapIn();
            GL.BufferSubData(Target, (IntPtr) offsetInBytes, data);
            GLException.ThrowGlErrors("Error writing data slice");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="data">The target span to fill.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void ReadData<T>(Span<T> data) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, get data, then swap back out.
            var revert = SwapIn();
            GL.GetBufferSubData(Target, IntPtr.Zero, data);
            GLException.ThrowGlErrors("Error reading data");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="data">The target span to fill.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void ReadData<T>(T[] data) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, get data, then swap back out.
            var revert = SwapIn();
            GL.GetBufferSubData(Target, IntPtr.Zero, data);
            GLException.ThrowGlErrors("Error reading data");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Reads a slice of the data.
        /// </summary>
        /// <param name="data">The target span to fill.</param>
        /// <param name="offsetInBytes">The offset at which to read.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void ReadData<T>(Span<T> data, int offsetInBytes) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, get data slice, then swap back out.
            var revert = SwapIn();
            GL.GetBufferSubData(Target, (IntPtr) offsetInBytes, data);
            GLException.ThrowGlErrors("Error reading data slice");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Reads a slice of the data.
        /// </summary>
        /// <param name="data">The target span to fill.</param>
        /// <param name="offsetInBytes">The offset at which to read.</param>
        /// <typeparam name="T">The type of the data.</typeparam>
        public void ReadData<T>(T[] data, int offsetInBytes) where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation, get data slice, then swap back out.
            var revert = SwapIn();
            GL.GetBufferSubData(Target, (IntPtr) offsetInBytes, data);
            GLException.ThrowGlErrors("Error reading data slice");
            SwapBackOut(revert);
        }

        /// <summary>
        /// Reads the entire data. 
        /// </summary>
        /// <typeparam name="T">The type of the data.</typeparam>
        /// <returns>Returns the data array.</returns>
        public T[] ReadAllData<T>() where T : unmanaged
        {
            // Ensure no fault.
            GLException.ThrowPreceding();

            // Swap buffer in for data operation.
            var revert = SwapIn();

            // Allocate target.
            var target = new T[SizeInBytes / Marshal.SizeOf<T>()];

            // Get buffer data fully.
            GL.GetBufferSubData(Target, IntPtr.Zero, target);
            GLException.ThrowGlErrors("Error reading all data");

            // Swap back out.
            SwapBackOut(revert);

            // Return the data.
            return target;
        }

        /// <summary>
        /// Binds the buffer. Does not check if there's an active buffer already.
        /// </summary>
        public void Bind() =>
            GL.BindBuffer(Target, Reference);

        /// <summary>
        /// Binds the buffer. If <paramref name="checkActive"/> is <c>true</c>, checks if there's an active buffer
        /// and throws an <see cref="InvalidOperationException"/> if there is and it's not this buffer.
        /// </summary>
        /// <param name="checkActive">True if current buffer should be checked for.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is another buffer active.</exception>
        public void Bind(bool checkActive)
        {
            // If no check needed, just use it.
            if (!checkActive)
            {
                GL.BindBuffer(Target, Reference);
                return;
            }

            // Get the current buffer. If same, return, if other, throw error.
            var current = Current(Target);
            if (current == Reference)
                return;
            if (current != 0)
                throw new InvalidOperationException("Other buffer currently active");

            // Use this buffer.
            GL.BindBuffer(Target, Reference);
        }

        /// <summary>
        /// Binds no buffer to the target if this buffer was active.
        /// </summary>
        public void Unbind()
        {
            if (Reference == Current(Target))
                BindNone(Target);
        }

        /// <summary>
        /// Deletes the buffer.
        /// </summary>
        protected override void DisposeContent() =>
            GL.DeleteBuffer(Reference);
    }
}