using System.Text;
using OpenTK.Graphics.OpenGL;
using UpNet.Graphics.Graphics.Shading;

namespace UpNet.Graphics.Graphics.Buffers
{
    /// <summary>
    /// Auxiliary class for pointer layout.
    /// </summary>
    public readonly struct PointerLayout
    {
        /// <summary>
        /// The number of attributes referenced.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// The component type.
        /// </summary>
        public VertexAttribPointerType Type { get; }

        /// <summary>
        /// True if the pointer should define normalized data.
        /// </summary>
        public bool Normalized { get; }

        /// <summary>
        /// The stride of data elements.
        /// </summary>
        public Unit Stride { get; }

        /// <summary>
        /// The offset of data elements.
        /// </summary>
        public Unit Offset { get; }

        /// <summary>
        /// Constructs the pointer layout with the given values.
        /// </summary>
        /// <param name="size">The number of attributes referenced.</param>
        /// <param name="type">The component type.</param>
        /// <param name="stride">The stride of data elements.</param>
        /// <param name="offset">The offset of data elements.</param>
        /// <param name="normalized">True if the pointer should define normalized data.</param>
        public PointerLayout(int size, VertexAttribPointerType type, Unit stride, Unit offset, bool normalized = false)
        {
            // Transfer all values.
            Size = size;
            Type = type;
            Normalized = normalized;
            Stride = stride;
            Offset = offset;
        }

        /// <summary>
        /// Constructs the pointer layout with the given values.
        /// </summary>
        /// <param name="size">The number of attributes referenced.</param>
        /// <param name="type">The component type.</param>
        /// <param name="stride">The stride of data elements.</param>
        /// <param name="normalized">True if the pointer should define normalized data.</param>
        public PointerLayout(int size, VertexAttribPointerType type, Unit stride, bool normalized = false)
            : this(size, type, stride, 0, normalized)
        {
            // Do nothing.
        }

        /// <summary>
        /// Constructs the pointer layout with the given values.
        /// </summary>
        /// <param name="size">The number of attributes referenced.</param>
        /// <param name="type">The component type.</param>
        /// <param name="normalized">True if the pointer should define normalized data.</param>
        public PointerLayout(int size, VertexAttribPointerType type, bool normalized = false)
            : this(size, type, 1.Elements(), 0, normalized)
        {
            // Do nothing.
        }


        /// <summary>
        /// Returns a representation of the <see cref="Uniform"/>, default values are omitted.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder($"PointerLayout, {nameof(Type)}: {Type}");

            if (1 != Size)
                result.Append($", {nameof(Size)}: {Size}");
            if (false != Normalized)
                result.Append($", {nameof(Normalized)}: {Normalized}");
            if (0 != Stride.Value)
                result.Append($", {nameof(Stride)}: {Stride}");
            if (0 != Offset.Value)
                result.Append($", {nameof(Offset)}: {Offset}");

            return result.ToString();
        }
    }
}