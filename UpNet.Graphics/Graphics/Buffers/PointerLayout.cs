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
        public int Stride { get; }

        /// <summary>
        /// The offset of data elements.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Constructs the pointer layout with the given values.
        /// </summary>
        /// <param name="size">The number of attributes referenced.</param>
        /// <param name="type">The component type.</param>
        /// <param name="normalized">True if the pointer should define normalized data.</param>
        /// <param name="stride">The stride of data elements.</param>
        /// <param name="offset">The offset of data elements.</param>
        public PointerLayout(int size, VertexAttribPointerType type, bool normalized, int stride, int offset)
        {
            // Transfer all values.
            Size = size;
            Type = type;
            Normalized = normalized;
            Stride = stride;
            Offset = offset;
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
            if (0 != Stride)
                result.Append($", {nameof(Stride)}: {Stride}");
            if (0 != Offset)
                result.Append($", {nameof(Offset)}: {Offset}");

            return result.ToString();
        }
    }
}