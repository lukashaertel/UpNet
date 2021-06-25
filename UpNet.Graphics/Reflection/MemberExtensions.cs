using System;
using System.Reflection;

namespace UpNet.Graphics.Reflection
{
    /// <summary>
    /// Extends <see cref="MemberInfo"/>.
    /// </summary>
    public static class MemberExtensions
    {
        /// <summary>
        /// Returns the default value for the member if it is either a <see cref="FieldInfo"/> or
        /// a <see cref="PropertyInfo"/>.
        /// </summary>
        /// <param name="member">The member to get the value for.</param>
        /// <returns>Returns the default value for the member.</returns>
        /// <exception cref="ArgumentException">Thrown when not a field or property.</exception>
        public static object? GetDefaultForFieldOrProperty(this MemberInfo member) =>
            member switch
            {
                // Fields return the field type's default value.
                FieldInfo field =>
                    field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null,

                // Properties return the property type's default value.
                PropertyInfo property =>
                    property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null,

                // Everything else is unknown.
                _ =>
                    throw new ArgumentException($"Member {member} is not a field or property", nameof(member))
            };
    }
}