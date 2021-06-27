namespace UpNet.Graphics.Graphics.Buffers
{
    /// <summary>
    /// Type of <see cref="Unit"/>.
    /// </summary>
    public enum UnitType : byte
    {
        /// <summary>
        /// Unit is in absolute bytes.
        /// </summary>
        Bytes,

        /// <summary>
        /// Unit is in number of components.
        /// </summary>
        Components,

        /// <summary>
        /// Unit is in entire elements.
        /// </summary>
        Elements
    }

    /// <summary>
    /// A numeric value with a unit.
    /// </summary>
    public readonly struct Unit
    {
        /// <summary>
        /// Returns the value as <see cref="UnitType.Bytes"/>.
        /// </summary>
        /// <param name="value">The value to use.</param>
        /// <returns>Returns a new <see cref="Unit"/>.</returns>
        public static implicit operator Unit(int value) => new(UnitType.Bytes, value);

        /// <summary>
        /// The type of the unit.
        /// </summary>
        public UnitType Type { get; }

        /// <summary>
        /// The numeric value of the unit.
        /// </summary>
        public int Value { get; }

        /// <summary>
        /// Creates a new instance of <see cref="Unit"/>.
        /// </summary>
        /// <param name="type">The type of the unit.</param>
        /// <param name="value">The numeric value of the unit.</param>
        public Unit(UnitType type, int value) =>
            (Type, Value) = (type, value);
    }

    /// <summary>
    /// Extends numbers to provide <see cref="Unit"/>.
    /// </summary>
    public static class UnitExtensions
    {
        /// <summary>
        /// Returns a <see cref="Unit"/> of <see cref="UnitType.Bytes"/>.
        /// </summary>
        /// <param name="value">The numeric value of the unit.</param>
        /// <returns>Returns a new instance of <see cref="Unit"/>.</returns>
        public static Unit Bytes(this int value) => new(UnitType.Bytes, value);

        /// <summary>
        /// Returns a <see cref="Unit"/> of <see cref="UnitType.Components"/>.
        /// </summary>
        /// <param name="value">The numeric value of the unit.</param>
        /// <returns>Returns a new instance of <see cref="Unit"/>.</returns>
        public static Unit Components(this int value) => new(UnitType.Components, value);

        /// <summary>
        /// Returns a <see cref="Unit"/> of <see cref="UnitType.Elements"/>.
        /// </summary>
        /// <param name="value">The numeric value of the unit.</param>
        /// <returns>Returns a new instance of <see cref="Unit"/>.</returns>
        public static Unit Elements(this int value) => new(UnitType.Elements, value);
    }
}