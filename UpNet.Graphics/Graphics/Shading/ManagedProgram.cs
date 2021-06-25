using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Constants;
using UpNet.Graphics.Lang;

namespace UpNet.Graphics.Graphics.Shading
{
    /// <summary>
    /// Managed interface to GL programs.
    /// </summary>
    public sealed class ManagedProgram : Managed<uint>
    {
        /// <summary>
        /// Gets the current program.
        /// </summary>
        public static uint Current
        {
            get
            {
                // Ensure no fault.
                GLException.ThrowPreceding();

                // Get current program, throw errors.
                var program = 0;
                GL.GetInteger(GetPName.CurrentProgram, ref program);
                GLException.ThrowGlErrors("Error getting current program");

                // Return program.
                return unchecked((uint) program);
            }
        }

        /// <summary>
        /// Uses no program.
        /// </summary>
        public static void UseNone() =>
            GL.UseProgram(0);

        /// <summary>
        /// All active <see cref="Uniform"/>.
        /// </summary>
        public IReadOnlyList<Uniform> Uniforms { get; private set; }

        /// <summary>
        /// All active <see cref="Attrib"/>.
        /// </summary>
        public IReadOnlyList<Attrib> Attribs { get; private set; }

        /// <summary>
        /// Creates a program and initializes it's GL reference.
        /// </summary>
        public ManagedProgram()
        {
            // Ensure no fault.
            GLException.ThrowPreceding();
            
            // Create program, initialize lists on empty.
            Reference = GL.CreateProgram();
            GLException.ThrowGlErrors("Error creating program");
            
            Uniforms = Array.AsReadOnly(Array.Empty<Uniform>());
            Attribs = Array.AsReadOnly(Array.Empty<Attrib>());
        }

        /// <summary>
        /// Adds a shader with source, compiles it and attaches it to the program.
        /// </summary>
        /// <param name="type">The type of the shader.</param>
        /// <param name="body">The source code.</param>
        /// <exception cref="CompileException">Thrown when compilation failed.</exception>
        public void AttachShader(ShaderType type, string body)
        {
            // Ensure no fault.
            GLException.ThrowPreceding();
            
            // Check state.
            CheckNotDisposed();

            // Create shader, source and compile, throw errors.
            var shader = GL.CreateShader(type);
            GL.ShaderSource(shader, body);
            GL.CompileShader(shader);
            GLException.ThrowGlErrors($"Error creating shader for type {type}");

            // If shader compilation failed, throw an exception.
            GL.GetShaderInfoLog(shader, out var info);
            if (string.Empty != info)
                throw new CompileException($"Error compiling shader for type {type}", info, type, body);

            // Attach the shader, throw errors.
            GL.AttachShader(Reference, shader);
            GLException.ThrowGlErrors($"Error creating shader for type {type}");
        }

        /// <summary>
        /// Links the program in it's current setup and retrieves the values
        /// for <see cref="Uniforms"/> and <see cref="Attribs"/>.
        /// </summary>
        public void Link()
        {
            // Ensure no fault.
            GLException.ThrowPreceding();
            
            // Check state.
            CheckNotDisposed();

            // Link program with the currently assigned set of shaders.
            GL.LinkProgram(Reference);
            GLException.ThrowGlErrors("Error linking program");

            // Detach and delete and all shaders.
            DetachAndDeleteShaders();

            // Retrieve shader uniforms and attribs.
            RetrieveUniforms();
            RetrieveAttribs();
        }

        /// <summary>
        /// Checks the object. If it's disposed, throws an exception.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown when <see cref="IsDisposed"/> is <c>true</c>.</exception>
        private void CheckNotDisposed()
        {
            // If disposed, throw an exception.
            if (IsDisposed)
                throw new ObjectDisposedException("The program is disposed", default(Exception?));
        }

        /// <summary>
        /// Gets the currently attached shaders, detaches and deletes them.
        /// </summary>
        private void DetachAndDeleteShaders()
        {
            // Get attached shaders, throw errors.
            var count = 0;
            Span<uint> attached = stackalloc uint[Sizes.AttachedShaderBufferSize];
            GL.GetAttachedShaders(Reference, ref count, attached);
            GLException.ThrowGlErrors("Error getting attached shaders");

            // Detach and delete all shaders, throw errors.
            foreach (var shader in attached[..count])
            {
                GL.DetachShader(Reference, shader);
                GL.DeleteShader(shader);
                GLException.ThrowGlErrors("Error detaching and deleting shader");
            }
        }

        /// <summary>
        /// Retrieves all uniforms.
        /// </summary>
        private void RetrieveUniforms()
        {
            // Get uniform count.
            var getCount = 0;
            GL.GetProgrami(Reference, ProgramPropertyARB.ActiveUniforms, ref getCount);

            // Targets for the get operations.
            var getUnit = 0;
            var getType = 0;
            var getSize = 0;
            var getBlockIndex = 0;
            var getOffset = 0;
            var getArrayStride = 0;
            var getMatrixStride = 0;
            var getIsRowMajor = 0;
            var getAtomicCounterBufferIndex = 0;

            // Make result uniforms from number of active uniforms.
            var uniforms = new Uniform[getCount];
            for (uint i = 0; i < getCount; i++)
            {
                // Get properties.
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformType, ref getType);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformSize, ref getSize);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformBlockIndex, ref getBlockIndex);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformOffset, ref getOffset);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformArrayStride, ref getArrayStride);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformMatrixStride, ref getMatrixStride);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformIsRowMajor, ref getIsRowMajor);
                GL.GetActiveUniformsi(Reference, 1, i,
                    UniformPName.UniformAtomicCounterBufferIndex, ref getAtomicCounterBufferIndex);

                // Get name.
                GL.GetActiveUniformName(Reference, i, Sizes.NameBufferSize,
                    ref getUnit, out string uniformName);

                // Get location.
                var location = GL.GetUniformLocation(Reference, uniformName);

                // Add new uniform.
                uniforms[i] = new Uniform(
                    location,
                    (UniformType) getType,
                    getSize,
                    getBlockIndex,
                    getOffset,
                    getArrayStride,
                    getMatrixStride,
                    1 == getIsRowMajor,
                    getAtomicCounterBufferIndex,
                    uniformName);
            }

            // Transfer to read only list.
            Uniforms = Array.AsReadOnly(uniforms);
        }

        /// <summary>
        /// Retrieves all attribs.
        /// </summary>
        private void RetrieveAttribs()
        {
            // Get attrib count.
            var getCount = 0;
            GL.GetProgrami(Reference, ProgramPropertyARB.ActiveAttributes, ref getCount);

            // Targets for the get operations.
            var getUnit = 0;
            var getSize = 0;
            var getType = default(AttributeType);

            // Make result attribs from number of active attribs.
            var attribs = new Attrib[getCount];
            for (uint i = 0; i < getCount; i++)
            {
                // Get properties and name.
                GL.GetActiveAttrib(Reference, i, Sizes.NameBufferSize,
                    ref getUnit, ref getSize, ref getType, out string attribName);

                // Get location.
                var location = GL.GetAttribLocation(Reference, attribName);

                // Add new attrib.
                attribs[i] = new Attrib(
                    location,
                    getSize,
                    getType,
                    attribName);
            }

            // Transfer to read only list.
            Attribs = Array.AsReadOnly(attribs);
        }

        /// <summary>
        /// Uses the program. Does not check if there's an active program already.
        /// </summary>
        public void Use() =>
            GL.UseProgram(Reference);

        /// <summary>
        /// Uses the program. If <paramref name="checkActive"/> is <c>true</c>, checks if there's an active program
        /// and throws an <see cref="InvalidOperationException"/> if there is and it's not this program.
        /// </summary>
        /// <param name="checkActive">True if current program should be checked for.</param>
        /// <exception cref="InvalidOperationException">Thrown if there is another program active.</exception>
        public void Use(bool checkActive)
        {
            // If no check needed, just use it.
            if (!checkActive)
            {
                GL.UseProgram(Reference);
                return;
            }

            // Get the current program. If same, return, if other, throw error.
            var current = Current;
            if (current == Reference)
                return;
            if (current != 0)
                throw new InvalidOperationException("Other program currently active");

            // Use this program.
            GL.UseProgram(Reference);
        }

        /// <summary>
        /// Uses no program if this program was active.
        /// </summary>
        public void Disuse()
        {
            if (Reference == Current)
                UseNone();
        }

        /// <summary>
        /// Gets the <see cref="Uniform"/> of the <paramref name="name"/> or returns <c>null</c>
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <returns>Returns the <see cref="Uniform"/> or <c>null</c></returns>
        public Uniform? FindUniform(string name)
        {
            // Check state.
            CheckNotDisposed();

            // Return first or default ordinal matching.
            return Uniforms.FirstOrDefault(uniform => name.Equals(uniform.Name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Gets the <see cref="Uniform"/> of the <paramref name="name"/> or throws
        /// a <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="name">The name of the uniform.</param>
        /// <returns>Returns the <see cref="Uniform"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the uniform was not found.</exception>
        public Uniform FindRequiredUniform(string name) =>
            FindUniform(name) ?? throw new KeyNotFoundException($"{name} is not a uniform of this program");


        /// <summary>
        /// Gets the <see cref="Attrib"/> of the <paramref name="name"/> or returns <c>null</c>
        /// </summary>
        /// <param name="name">The name of the attrib.</param>
        /// <returns>Returns the <see cref="Attrib"/> or <c>null</c></returns>
        public Attrib? FindAttrib(string name)
        {
            // Check state.
            CheckNotDisposed();

            // Return first or default ordinal matching.
            return Attribs.FirstOrDefault(attrib => name.Equals(attrib.Name, StringComparison.Ordinal));
        }

        /// <summary>
        /// Gets the <see cref="Attrib"/> of the <paramref name="name"/> or throws
        /// a <see cref="KeyNotFoundException"/>.
        /// </summary>
        /// <param name="name">The name of the attrib.</param>
        /// <returns>Returns the <see cref="Attrib"/>.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the attrib was not found.</exception>
        public Attrib FindRequiredAttrib(string name) =>
            FindAttrib(name) ?? throw new KeyNotFoundException($"{name} is not a attrib of this program");

        /// <summary>
        /// Gets the program's uniform with the given name. Delegates to <see cref="FindRequiredUniform"/>.
        /// </summary>
        /// <param name="name">The name of the uniform to find.</param>
        public Uniform this[string name] =>
            FindRequiredUniform(name);

        /// <summary>
        /// Detach and delete the shaders.
        /// </summary>
        protected override void DisposeContent() =>
            DetachAndDeleteShaders();
    }
}