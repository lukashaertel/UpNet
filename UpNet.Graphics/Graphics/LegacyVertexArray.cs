using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Graphics.Attributes;
using UpNet.Graphics.Graphics.Shading;
using UpNet.Graphics.Handles;
using UpNet.Graphics.Reflection;

namespace UpNet.Graphics.Graphics
{
    /// <summary>
    /// A GL vertex array object. Resolves <see cref="BufferAttribute"/>. Generates and disposes of vertex array and the
    /// associated buffers.
    /// </summary>
    /// <remarks>
    /// All buffers for the attribs should be properly assigned, otherwise the application might crash without a
    /// proper error notification. Use <see cref="GL.BindBuffer"/> and
    /// <see cref="GL.BufferData(OpenTK.Graphics.OpenGL.BufferTargetARB,nint,void*,OpenTK.Graphics.OpenGL.BufferUsageARB)"/>
    /// accordingly.
    /// </remarks>
    [Obsolete]
    public class LegacyVertexArray : IDisposable
    {
        /// <summary>
        /// True if the vertex array was attached to is principal content.
        /// </summary>
        public bool Attached { get; private set; }

        /// <summary>
        /// The reference to the GL VAO.
        /// </summary>
        public uint Reference { get; private set; }

        /// <summary>
        /// Backing field for <see cref="AllBuffers"/>, locally mutable.
        /// </summary>
        private readonly List<uint> _allBuffers = new();

        /// <summary>
        /// All buffers that were generated for this vertex array.
        /// </summary>
        public IReadOnlyList<uint> AllBuffers => _allBuffers.AsReadOnly();

        private readonly List<uint> _allEnabledAttribs = new();

        /// <summary>
        /// All attribs that were enabled for this vertex array.
        /// </summary>
        public IReadOnlyList<uint> AllEnabledAttribs => _allEnabledAttribs.AsReadOnly();

        /// <summary>
        /// Creates the vertex array. Resolves the attributes.
        /// </summary>
        protected LegacyVertexArray()
        {
            // Generate vertex array.
            Reference = GL.GenVertexArray();

            // Resolve all buffer attributes.
            this.ResolveAttributeValues<BufferAttribute>(_ =>
            {
                // Generate buffer and add to list of all buffers before returning it.
                var buffer = GL.GenBuffer();
                _allBuffers.Add(buffer);
                return buffer;
            });
        }

        /// <summary>
        /// Uses the vertex array and returns the open vertex array handle.
        /// </summary>
        /// <returns>Returns the open vertex array handle.</returns>
        public OpenVertexArray Bind() => new(Reference);

        /// <summary>
        /// Disables all currently enabled attribs.
        /// </summary>
        public void Detach()
        {
            // Skip if already detached.
            if (!Attached)
                return;

            // Bind no buffer.
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, 0);

            // Disable all previously enabled attribs and reset pointer data.
            foreach (var attrib in _allEnabledAttribs)
            {
                GL.VertexAttribPointer(attrib, 0, default, false, 0, 0);
                GL.DisableVertexAttribArray(attrib);
            }

            // Clear the list of enabled attribs.
            _allEnabledAttribs.Clear();

            // Mark as not attached.
            Attached = false;
        }

        /// <summary>
        /// Sets the vertex attrib pointer layout and enables the vertex attrib arrays. Matches the target locations
        /// for the given program.
        /// </summary>
        /// <param name="program">The program to enable for.</param>
        /// <exception cref="ArgumentException">
        /// Throws an argument exception if the program does not contain a matching field or property holding the
        /// attrib location.
        /// </exception>
        public void Attach(LegacyProgram program)
        {
            // Make sure principal content is detached.
            Detach();

            // Get program type.
            var programType = program.GetType();

            // Run on each member with the vertex attrib attribute. Convert the member values to uint.
            this.ForEachAttributedMember<LayoutAttribute, uint>((attribute, member, buffer) =>
            {
                // Only run on target shader type if limited.
                if (attribute.ForShader != null && programType != attribute.ForShader)
                    return;

                // Get the target index for the given shader.
                var target = programType.GetField(attribute.ForAttrib)?.GetValue(program) ??
                             programType.GetProperty(attribute.ForAttrib)?.GetValue(program);

                // If the target could not be found, throw an exception.
                if (null == target)
                    throw new ArgumentException($"{attribute.ForAttrib} not found for {member.Name}", nameof(program),
                        new InvalidOperationException("Invalid attrib specified for vertex attrib"));

                // Convert for indexing.
                var index = Convert.ToInt32(target);
                if (index < 0)
                {
                    // Compose message.
                    var message = $"{attribute.ForAttrib} for {member.Name} not resolved in program";

                    // Log or throw.
                    if (!attribute.Required) Console.Error.WriteLine(message, "Warning");
                    else throw new InvalidOperationException(message);
                }
                else
                {
                    // Bind buffer to assign state.
                    GL.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);

                    // Send layout.
                    GL.VertexAttribPointer((uint) index,
                        attribute.Size,
                        attribute.Type,
                        attribute.Normalized,
                        attribute.Stride,
                        attribute.Offset);

                    if (0 < attribute.Divisor)
                        GL.VertexAttribDivisor((uint) index, attribute.Divisor);

                    // Enable the attrib.
                    GL.EnableVertexAttribArray((uint) index);

                    // Add to enabled attribs.
                    _allEnabledAttribs.Add((uint) index);
                }
            });

            // Mark as attached.
            Attached = true;
        }


        public void Attach(ManagedProgram program)
        {
            // Make sure principal content is detached.
            Detach();

            // Run on each member with the vertex attrib attribute. Convert the member values to uint.
            this.ForEachAttributedMember<LayoutAttribute, uint>((attribute, member, buffer) =>
            {
                // Only run on target shader type if limited.
                if (attribute.ForShader != null)
                    return;

                // Get the target index for the given shader.
                var target = program.FindAttrib(attribute.ForAttrib)?.Location;

                // If the target could not be found, throw an exception.
                if (null == target)
                    throw new ArgumentException($"{attribute.ForAttrib} not found for {member.Name}", nameof(program),
                        new InvalidOperationException("Invalid attrib specified for vertex attrib"));

                // Convert for indexing.
                var index = Convert.ToInt32(target);
                if (index < 0)
                {
                    // Compose message.
                    var message = $"{attribute.ForAttrib} for {member.Name} not resolved in program";

                    // Log or throw.
                    if (!attribute.Required) Console.Error.WriteLine(message, "Warning");
                    else throw new InvalidOperationException(message);
                }
                else
                {
                    // Bind buffer to assign state.
                    GL.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);

                    // Send layout.
                    GL.VertexAttribPointer((uint) index,
                        attribute.Size,
                        attribute.Type,
                        attribute.Normalized,
                        attribute.Stride,
                        attribute.Offset);

                    if (0 < attribute.Divisor)
                        GL.VertexAttribDivisor((uint) index, attribute.Divisor);

                    // Enable the attrib.
                    GL.EnableVertexAttribArray((uint) index);

                    // Add to enabled attribs.
                    _allEnabledAttribs.Add((uint) index);
                }
            });

            // Mark as attached.
            Attached = true;
        }

        /// <summary>
        /// True if the vertex array and the buffers were already disposed of.
        /// </summary>
        public bool IsDisposed => uint.MaxValue == Reference;

        /// <summary>
        /// Disposes of the underlying GL vertex array and buffers.
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
                // Delete vertex array and set disposed.
                GL.DeleteVertexArray(Reference);
                Reference = uint.MaxValue;

                // Delete all buffers and set disposed.
                for (var i = 0; i < _allBuffers.Count; i++)
                {
                    GL.DeleteBuffer(_allBuffers[i]);
                    _allBuffers[i] = uint.MaxValue;
                }
            }
        }

        /// <summary>
        /// Disposes of the underlying GL vertex array and buffers.
        /// </summary>
        public void Dispose()
        {
            // Dispose from dispose and suppress finalization.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}