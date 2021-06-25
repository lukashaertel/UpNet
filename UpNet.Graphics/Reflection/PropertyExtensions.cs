using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UpNet.Graphics.Reflection
{
    /// <summary>
    /// Extends <see cref="PropertyInfo"/>.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        /// Binding flags to look for when finding the backing field in <see cref="SetBackingFieldValue"/>.
        /// </summary>
        private const BindingFlags BackingFieldAttributes =
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// Tries to find the backing field for the property and assign it's value.
        /// </summary>
        /// <param name="propertyInfo">The property to set.</param>
        /// <param name="obj">The target object.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the declaring type is not available or the backing field could not be found.
        /// </exception>
        public static void SetBackingFieldValue(this PropertyInfo propertyInfo, object? obj, object? value)
        {
            // Get the declaring type and throw if it cannot be resolved.
            var type = propertyInfo.DeclaringType ??
                       throw new ArgumentException($"Property's declaring type not available", nameof(propertyInfo));

            // Compose the prefix to look for.
            var prefix = $"<{propertyInfo.Name}>";

            // Check all candidate fields.
            foreach (var field in type.GetFields(BackingFieldAttributes))
            {
                // Attributes mismatch, skip.
                if (!field.Attributes.HasFlag(FieldAttributes.Private))
                    continue;
                if (!field.Attributes.HasFlag(FieldAttributes.InitOnly))
                    continue;

                // Not compiler generated, skip.
                if (field.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
                    continue;

                // Not declared in the same type, skip.
                if (field.DeclaringType != propertyInfo.DeclaringType)
                    continue;

                // Mismatching type, skip.
                if (!field.FieldType.IsAssignableFrom(propertyInfo.PropertyType))
                    continue;

                // Not backing field name pattern, skip.
                if (!field.Name.StartsWith(prefix))
                    continue;

                // Found, set value and return.
                field.SetValue(obj, value);
                return;
            }

            // Couldn't find anything, throw an exception.
            throw new ArgumentException($"Property's backing field could not be resolved");
        }
    }
}