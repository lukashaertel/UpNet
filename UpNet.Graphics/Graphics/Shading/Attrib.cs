using System.Text;
using OpenTK.Graphics.OpenGL;

namespace UpNet.Graphics.Graphics.Shading
{
    /// <summary>
    /// The data of an Attrib.
    /// </summary>
    public readonly struct Attrib
    {
        /// <summary>
        /// Returns the attrib's location.
        /// </summary>
        public static implicit operator int(Attrib attrib) => attrib.Location;

        /// <summary>
        /// Returns an attrib for the location only. All other fields are not meaningful.
        /// </summary>
        /// <param name="location">The location to use.</param>
        /// <returns>Returns a new <see cref="Attrib"/>.</returns>
        public static implicit operator Attrib(int location) => new(
            location,
            default,
            default,
            $"{location}???");

        /// <summary>
        /// The location of the <see cref="Attrib"/>.
        /// </summary>
        public int Location { get; }

        /// <summary>
        /// Size of the <see cref="Attrib"/>.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Type of the <see cref="Attrib"/>.
        /// </summary>
        public AttributeType Type { get; }

        /// <summary>
        /// Name of the <see cref="Attrib"/>.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Constructs the <see cref="Attrib"/> with the given values.
        /// </summary>
        /// <param name="location">The location of the <see cref="Attrib"/>.</param>
        /// <param name="size">Size of the <see cref="Attrib"/>.</param>
        /// <param name="type">Type of the <see cref="Attrib"/>.</param>
        /// <param name="name">Name of the <see cref="Attrib"/>.</param>
        public Attrib(int location, int size, AttributeType type, string name)
        {
            // Transfer all values.
            Location = location;
            Size = size;
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Returns a representation of the <see cref="Attrib"/>, default values are omitted.
        /// </summary>
        public override string ToString()
        {
            var result = new StringBuilder($"Attrib {Name}@{Location}, {nameof(Type)}: {Type}");

            if (1 != Size)
                result.Append($", {nameof(Size)}: {Size}");

            return result.ToString();
        }
    }
}