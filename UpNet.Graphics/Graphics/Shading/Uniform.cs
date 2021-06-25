using System.Text;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Graphics.Shading
{
    /// <summary>
    /// The data of a Uniform.
    /// </summary>
    public readonly struct Uniform
    {
        /// <summary>
        /// Returns the uniform's location.
        /// </summary>
        public static implicit operator int(Uniform uniform) => uniform.Location;

        /// <summary>
        /// Returns an uniform for the location only. All other fields are not meaningful.
        /// </summary>
        /// <param name="location">The location to use.</param>
        /// <returns>Returns a new <see cref="Uniform"/>.</returns>
        public static implicit operator Uniform(int location) => new(
            location,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            default,
            $"{location}???");

        /// <summary>
        /// The location of the <see cref="Uniform"/>.
        /// </summary>
        public int Location { get; }

        /// <summary>
        /// Type of the <see cref="Uniform"/>.
        /// </summary>
        public UniformType Type { get; }

        /// <summary>
        /// Size of the <see cref="Uniform"/>.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Block index of the <see cref="Uniform"/>.
        /// </summary>
        public int BlockIndex { get; }

        /// <summary>
        /// Offset of the <see cref="Uniform"/>.
        /// </summary>
        public int Offset { get; }

        /// <summary>
        /// Array stride of the <see cref="Uniform"/>.
        /// </summary>
        public int ArrayStride { get; }

        /// <summary>
        /// Matrix stride of the <see cref="Uniform"/>.
        /// </summary>
        public int MatrixStride { get; }

        /// <summary>
        /// True if matrix is row-major <see cref="Uniform"/>.
        /// </summary>
        public bool IsRowMajor { get; }

        /// <summary>
        /// Atomic counter buffer index of the <see cref="Uniform"/>.
        /// </summary>
        public int AtomicCounterBufferIndex { get; }

        /// <summary>
        /// Name of the <see cref="Uniform"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructs the <see cref="Uniform"/> with the given values.
        /// </summary>
        /// <param name="location">The location of the <see cref="Uniform"/>.</param>
        /// <param name="type">Type of the <see cref="Uniform"/>.</param>
        /// <param name="size">Size of the <see cref="Uniform"/>.</param>
        /// <param name="blockIndex">Block index of the <see cref="Uniform"/>.</param>
        /// <param name="offset">Offset of the <see cref="Uniform"/>.</param>
        /// <param name="arrayStride">Array stride of the <see cref="Uniform"/>.</param>
        /// <param name="matrixStride">Matrix stride of the <see cref="Uniform"/>.</param>
        /// <param name="isRowMajor">True if matrix is row-major <see cref="Uniform"/>.</param>
        /// <param name="atomicCounterBufferIndex">Atomic counter buffer index of the <see cref="Uniform"/>.</param>
        /// <param name="name">Name of the <see cref="Uniform"/>.</param>
        public Uniform(
            int location,
            UniformType type,
            int size,
            int blockIndex,
            int offset,
            int arrayStride,
            int matrixStride,
            bool isRowMajor,
            int atomicCounterBufferIndex,
            string name)
        {
            // Transfer all values.
            Location = location;
            Type = type;
            Size = size;
            BlockIndex = blockIndex;
            Offset = offset;
            ArrayStride = arrayStride;
            MatrixStride = matrixStride;
            IsRowMajor = isRowMajor;
            AtomicCounterBufferIndex = atomicCounterBufferIndex;
            Name = name;
        }

        /// <summary>
        /// Returns a representation of the <see cref="Uniform"/>, default values are omitted.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder($"Uniform {Name}@{Location}, {nameof(Type)}: {Type}");

            if (1 != Size)
                result.Append($", {nameof(Size)}: {Size}");
            if (-1 != BlockIndex)
                result.Append($", {nameof(BlockIndex)}: {BlockIndex}");
            if (-1 != Offset)
                result.Append($", {nameof(Offset)}: {Offset}");
            if (-1 != ArrayStride)
                result.Append($", {nameof(ArrayStride)}: {ArrayStride}");
            if (-1 != MatrixStride)
                result.Append($", {nameof(MatrixStride)}: {MatrixStride}");
            if (false != IsRowMajor)
                result.Append($", {nameof(IsRowMajor)}: {IsRowMajor}");
            if (-1 != AtomicCounterBufferIndex)
                result.Append($", {nameof(AtomicCounterBufferIndex)}: {AtomicCounterBufferIndex}");

            return result.ToString();
        }
    }
}