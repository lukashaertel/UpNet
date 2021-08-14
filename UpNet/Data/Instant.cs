using System;

namespace UpNet.Data
{
    /// <summary>
    /// Exchangeable instant.
    /// </summary>
    /// <param name="At">The wall-clock time.</param>
    /// <param name="Inner">The in-player disambiguation.</param>
    /// <param name="Player">The originating player.</param>
    public record Instant(DateTime At, byte Inner, Guid Player) : IComparable<Instant>
    {
        /// <summary>
        /// Minimum <see cref="Instant"/> value/
        /// </summary>
        public static readonly Instant MinValue = new(DateTime.MinValue, byte.MinValue, new Guid(
            int.MinValue, short.MinValue, short.MinValue, byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue,
            byte.MinValue, byte.MinValue, byte.MinValue, byte.MinValue));

        /// <summary>
        /// Maximum <see cref="Instant"/> value/
        /// </summary>
        public static readonly Instant MaxValue = new(DateTime.MaxValue, byte.MaxValue, new Guid(
            int.MaxValue, short.MaxValue, short.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue,
            byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));

        public int CompareTo(Instant? other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var atComparison = At.CompareTo(other.At);
            if (atComparison != 0) return atComparison;
            var innerComparison = Inner.CompareTo(other.Inner);
            if (innerComparison != 0) return innerComparison;
            return Player.CompareTo(other.Player);
        }

        public virtual bool Equals(Instant? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return At.Equals(other.At) && Inner == other.Inner && Player.Equals(other.Player);
        }

        public override int GetHashCode() =>
            HashCode.Combine(At, Inner, Player);

        public override string ToString() =>
            $"{At}#{Inner}@{Player}";
    }
}