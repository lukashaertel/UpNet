using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Constants;
using UpNet.Graphics.Graphics.Attributes;
using UpNet.Graphics.Handles;
using UpNet.Graphics.Reflection;

namespace UpNet.Graphics.Graphics
{
    /// <summary>
    /// A GL program object. Resolves <see cref="UniformAttribute"/> and <see cref="AttribAttribute"/>. Generates and
    /// disposes of a GL program handle.
    /// </summary>
    [Obsolete]
    public class LegacyProgram : IDisposable
    {
        /// <summary>
        /// True if the program was attached to is principal content.
        /// </summary>
        public bool Attached { get; private set; }

        /// <summary>
        /// The reference to the GL program.
        /// </summary>
        public uint Reference { get; private set; }

        /// <summary>
        /// Creates a shader program with a set of sources. Resolves the attributes.
        /// </summary>
        /// <param name="shaders">The shader sources.</param>
        /// <exception cref="ArgumentException">
        /// Throws an argument exception if one of the shaders could not be compiled.
        /// </exception>
        protected LegacyProgram()
        {
            // Create the target program.
            Reference = GL.CreateProgram();
        }

        public void Detach()
        {
            // Skip if already detached.
            if (!Attached)
                return;

            // Reset values.
            this.ResolveAttributeValues<UniformAttribute>((_, member) => member.GetDefaultForFieldOrProperty());
            this.ResolveAttributeValues<AttribAttribute>((_, member) => member.GetDefaultForFieldOrProperty());

            // Mark as not attached.
            Attached = false;
        }

        public void Attach(params LegacyProgramArg[] shaders)
        {
            // Make sure principal content is detached.
            Detach();

            // List of references to the shader programs to later attach, detach, and clean up.
            Span<uint> shadersReferences = stackalloc uint[shaders.Length];

            // Compile all shaders.
            for (var i = 0; i < shaders.Length; i++)
            {
                // Get source material.
                var shader = shaders[i];

                // Get new reference, memorize it.
                var reference = shadersReferences[i] = GL.CreateShader(shader.Type);

                // Attach shader source and compile it.
                GL.ShaderSource(reference, shader.Source);
                GL.CompileShader(reference);

                // If shader compilation failed, throw an exception.
                GL.GetShaderInfoLog(reference, out var info);
                if (string.Empty != info)
                    throw new ArgumentException($"Error compiling shader {shader.Type}", nameof(shaders),
                        new Exception(info));
            }

            // Attach all shaders.
            foreach (var reference in shadersReferences)
                GL.AttachShader(Reference, reference);

            // Link program with the currently assigned set of shaders.
            GL.LinkProgram(Reference);

            // Clean up shaders.
            foreach (var reference in shadersReferences)
            {
                GL.DetachShader(Reference, reference);
                GL.DeleteShader(reference);
            }

            // Resolve uniform attributes.
            this.ResolveAttributeValues<UniformAttribute>((uniform, member) =>
            {
                // Get location for uniform. Throw if it could not be found and marked required.
                var location = GL.GetUniformLocation(Reference, uniform.Named ?? member.Name);
                if (location < 0)
                {
                    // Compose message.
                    var message = uniform.Named == null
                        ? $"Uniform for {member.Name} could not be resolved"
                        : $"Uniform {uniform.Named} for {member.Name} could not be resolved";

                    // Log or throw.
                    if (!uniform.Required) Console.Error.WriteLine(message);
                    else throw new InvalidOperationException(message);
                }

                return location;
            });

            // Resolve attrib attributes.
            this.ResolveAttributeValues<AttribAttribute>((attrib, member) =>
            {
                // Get location for attrib. Throw if it could not be found and marked required.
                var location = GL.GetAttribLocation(Reference, attrib.Named ?? member.Name);
                if (location < 0)
                {
                    // Compose message.
                    var message = attrib.Named == null
                        ? $"Attrib for {member.Name} could not be resolved"
                        : $"Attrib {attrib.Named} for {member.Name} could not be resolved";

                    // Log or throw.
                    if (!attrib.Required) Console.Error.WriteLine(message);
                    else throw new InvalidOperationException(message);
                }

                return location;
            });

            // Mark as attached.
            Attached = true;
        }

        /// <summary>
        /// Retrieves the active attribute names for the program.
        /// </summary>
        public IReadOnlyList<string> AllAttribNames
        {
            get
            {
                var count = 0;
                GL.GetProgrami(Reference, ProgramPropertyARB.ActiveAttributes, ref count);
                var result = new string[count];
                var length = 0;
                var size = 0;
                var type = default(AttributeType);
                for (uint i = 0; i < count; i++)
                    GL.GetActiveAttrib(Reference, i, Sizes.NameBufferSize,
                        ref length, ref size, ref type, out result[i]);
                return Array.AsReadOnly(result);
            }
        }

        /// <summary>
        /// Retrieves the active uniform names for the program.
        /// </summary>
        public IReadOnlyList<string> AllUniformNames
        {
            get
            {
                var count = 0;
                GL.GetProgrami(Reference, ProgramPropertyARB.ActiveUniforms, ref count);
                var result = new string[count];
                var length = 0;
                for (uint i = 0; i < count; i++)
                    GL.GetActiveUniformName(Reference, i, Sizes.NameBufferSize,
                        ref length, out result[i]);
                return Array.AsReadOnly(result);
            }
        }

        /// <summary>
        /// Uses the program and returns the open program handle.
        /// </summary>
        /// <returns>Returns the open program handle.</returns>
        public OpenProgram Bind() => new(Reference);

        /// <summary>
        /// True if the program was already disposed of.
        /// </summary>
        public bool IsDisposed => uint.MaxValue == Reference;

        /// <summary>
        /// Disposes of the underlying GL program.
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
                // Delete program and set disposed.
                GL.DeleteProgram(Reference);
                Reference = uint.MaxValue;
            }
        }

        /// <summary>
        /// Disposes of the underlying GL program.
        /// </summary>
        public void Dispose()
        {
            // Dispose from dispose and suppress finalization.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}