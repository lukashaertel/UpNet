using System;

namespace UpNet.Graphics.Graphics.Attributes
{
    /// <summary>
    /// Marks a field or property as the target of uniform resolution.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class UniformAttribute : Attribute
    {
        /// <summary>
        /// If given, overrides the name to look for.
        /// </summary>
        public string? Named { get; }

        /// <summary>
        /// If given true, throws an exception when not found.
        /// </summary>
        public bool Required { get; }

        /// <summary>
        /// Creates the <see cref="UniformAttribute"/>.
        /// </summary>
        /// <param name="named">If given, overrides the name to look for.</param>
        /// <param name="required">If given true, throws an exception when not found.</param>
        public UniformAttribute(string? named = null, bool required = false)
        {
            // Check argument.
            if (null != named && string.Empty == named)
                throw new ArgumentException("Must be either null or not empty", nameof(named));

            Named = named;
            Required = required;
        }
    }
}