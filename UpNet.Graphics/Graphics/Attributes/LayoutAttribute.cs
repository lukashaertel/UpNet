using System;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Graphics.Attributes
{
    /// <summary>
    /// Marks a field or property as the target of vertex attrib resolution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public sealed class LayoutAttribute : Attribute
    {
        /// <summary>
        /// The type of the <see cref="LegacyProgram"/>.
        /// </summary>
        public Type? ForShader { get; }

        /// <summary>
        /// The name to match in the <see cref="LegacyProgram"/>.
        /// </summary>
        public string ForAttrib { get; }

        /// <summary>
        /// The number of components.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The type of the components.
        /// </summary>
        public VertexAttribPointerType Type { get; }

        /// <summary>
        /// True if normalized values.
        /// </summary>
        public bool Normalized { get; }

        /// <summary>
        /// If given, overrides the stride.
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// The offset into the data.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// The divisor for this layout attrib.
        /// </summary>
        public uint Divisor { get; }

        /// <summary>
        /// If given true, throws an exception when not found.
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// Creates the <see cref="LayoutAttribute"/>.
        /// </summary>
        /// <param name="forShader">The name to match in the <see cref="LegacyProgram"/>.</param>
        /// <param name="forAttrib">The type of the <see cref="LegacyProgram"/>.</param>
        /// <param name="size">The number of components.</param>
        /// <param name="type">The type of the components.</param>
        /// <param name="normalized">True if normalized values.</param>
        /// <param name="stride">If given, overrides the stride.</param>
        /// <param name="offset">The offset into the data.</param>
        /// <param name="required">If given true, throws an exception when not found.</param>
        /// <param name="divisor">The divisor for this attrib.</param>
        public LayoutAttribute(
            Type? forShader,
            string forAttrib,
            int size,
            VertexAttribPointerType type,
            bool normalized = false,
            int stride = int.MinValue,
            int offset = 0,
            bool required = false,
            uint divisor = 0)
        {
            // Check arguments.
            if (false == forShader?.IsAssignableTo(typeof(LegacyProgram)))
                throw new ArgumentException("Must be assignable to Program", nameof(forShader));
            if (string.Empty == forAttrib)
                throw new ArgumentException("Must not be empty", nameof(forAttrib));
            if (size <= 0)
                throw new ArgumentException("Must be positive", nameof(size));
            if (int.MinValue != stride && stride <= 0)
                throw new ArgumentException("Must be int.MinValue or not be negative", nameof(stride));
            if (offset < 0)
                throw new ArgumentException("Must not be negative", nameof(offset));

            // Set basic properties.
            ForShader = forShader;
            ForAttrib = forAttrib;
            Size = size;
            Type = type;
            Normalized = normalized;
            Offset = offset;
            Required = required;
            Divisor = divisor;

            // Decide on how to work on stride.
            if (stride != int.MinValue)
                // Stride is set, assign.
                Stride = stride;
            else
                // Stride is not set, compute.
                Stride = type switch
                {
                    VertexAttribPointerType.Byte => size * sizeof(sbyte),
                    VertexAttribPointerType.UnsignedByte => size * sizeof(byte),
                    VertexAttribPointerType.Short => size * sizeof(short),
                    VertexAttribPointerType.UnsignedShort => size * sizeof(ushort),
                    VertexAttribPointerType.Int => size * sizeof(int),
                    VertexAttribPointerType.UnsignedInt => size * sizeof(uint),
                    VertexAttribPointerType.Float => size * sizeof(float),
                    VertexAttribPointerType.Double => size * sizeof(double),
                    VertexAttribPointerType.UnsignedInt2101010Rev => size * ((2 + 10 + 10 + 10) / 8),
                    VertexAttribPointerType.UnsignedInt10f11f11fRev => size * ((10 + 11 + 11) / 8),
                    VertexAttribPointerType.HalfFloat => size * sizeof(ushort),
                    VertexAttribPointerType.Int2101010Rev => size * ((2 + 10 + 10 + 10) / 8),
                    VertexAttribPointerType.Fixed => size * ((16 + 16) / 8),
                    VertexAttribPointerType.Int64Nv => size * sizeof(long),
                    VertexAttribPointerType.UnsignedInt64Nv => size * sizeof(ulong),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
                };
        }

        /// <summary>
        /// Creates the <see cref="LayoutAttribute"/>. Does not restrict <see cref="ForShader"/>.
        /// </summary>
        /// <param name="forAttrib">The type of the <see cref="LegacyProgram"/>.</param>
        /// <param name="size">The number of components.</param>
        /// <param name="type">The type of the components.</param>
        /// <param name="normalized">True if normalized values.</param>
        /// <param name="stride">If given, overrides the stride.</param>
        /// <param name="offset">The offset into the data.</param>
        /// <param name="required">If given true, throws an exception when not found.</param>
        public LayoutAttribute(
            string forAttrib,
            int size,
            VertexAttribPointerType type,
            bool normalized = false,
            int stride = int.MinValue,
            int offset = 0,
            bool required = false) : this(null, forAttrib, size, type, normalized, stride, offset, required)
        {
            // Everything handled by full constructor.
        }
    }
}