using System;
using System.Reflection;

namespace UpNet.Graphics.Reflection
{
    /// <summary>
    /// Extends objects that hold <see cref="Attribute"/> fields and properties.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Resolves the values of fields or properties annotated with <typeparamref name="T"/>. Discards the member
        /// information for cases where it is not needed.
        /// </summary>
        /// <param name="target">
        /// The receiving object in which to look for fields and properties and for which to resolve the values.
        /// </param>
        /// <param name="resolver">The resolution function.</param>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        public static void ResolveAttributeValues<T>(this object target, Func<T, object?> resolver)
            where T : Attribute =>
            ResolveAttributeValues<T>(target, (a, _) => resolver(a));

        /// <summary>
        /// Resolves the values of fields or properties annotated with <typeparamref name="T"/>.
        /// </summary>
        /// <param name="target">
        /// The receiving object in which to look for fields and properties and for which to resolve the values.
        /// </param>
        /// <param name="resolver">The resolution function.</param>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        public static void ResolveAttributeValues<T>(this object target, Func<T, MemberInfo, object?> resolver)
            where T : Attribute
        {
            // Get target and attribute type.
            var type = target.GetType();
            var attributeType = typeof(T);

            // Resolve attribute for fields.
            foreach (var field in type.GetFields())
            {
                // Try to get attribute, if not found, skip this field.
                var attribute = field.GetCustomAttribute(attributeType);
                if (attribute == null)
                    continue;

                // Get value from resolver and change it's type to match field.
                var value = Convert.ChangeType(resolver((T) attribute, field), field.FieldType);

                // Set field value.
                field.SetValue(target, value);
            }

            // Resolve attribute for properties.
            foreach (var property in type.GetProperties())
            {
                // Try to get attribute, if not found, skip this property.
                var attribute = property.GetCustomAttribute(attributeType);
                if (attribute == null)
                    continue;

                // Get value from resolver and change it's type to match property.
                var value = Convert.ChangeType(resolver((T) attribute, property),
                    property.PropertyType);

                // Set with appropriate method.
                if (null != property.SetMethod)
                    property.SetValue(target, value);
                else
                    property.SetBackingFieldValue(target, value);
            }
        }

        /// <summary>
        /// Runs the action on all fields or properties annotated with <typeparamref name="T"/> with their current
        /// value. Discards the member information for cases where it is not needed.
        /// </summary>
        /// <param name="target">
        /// The receiving object in which to look for fields and properties and for which to run the action.
        /// </param>
        /// <param name="action">The action to run.</param>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        public static void ForEachAttributedMember<T>(this object target, Action<T, object?> action)
            where T : Attribute =>
            ForEachAttributedMember<T>(target, (t, _, v) => action(t, v));

        /// <summary>
        /// Runs the action on all fields or properties annotated with <typeparamref name="T"/> with their current
        /// value.
        /// </summary>
        /// <param name="target">
        /// The receiving object in which to look for fields and properties and for which to run the action.
        /// </param>
        /// <param name="action">The action to run.</param>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        public static void ForEachAttributedMember<T>(this object target, Action<T, MemberInfo, object?> action)
            where T : Attribute
        {
            // Get target and attribute type.
            var type = target.GetType();
            var attributeType = typeof(T);

            // Resolve attribute for fields.
            foreach (var field in type.GetFields())
            {
                // Try to get attributes, run action with collected data and field value.
                foreach (var attribute in field.GetCustomAttributes(attributeType))
                    action((T) attribute, field, field.GetValue(target));
            }

            // Resolve attribute for properties.
            foreach (var property in type.GetProperties())
            {
                // Try to get attributes, run action with collected data and field value.
                foreach (var attribute in property.GetCustomAttributes(attributeType))
                    action((T) attribute, property, property.GetValue(target));
            }
        }


        /// <summary>
        /// Runs the action on all fields or properties annotated with <typeparamref name="T"/> with their current
        /// value. Converts the member values for processing. Discards the member information for cases where it is not
        /// needed.
        /// </summary>
        /// <param name="target">
        /// The receiving object in which to look for fields and properties and for which to run the action.
        /// </param>
        /// <param name="action">The action to run.</param>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        /// <typeparam name="TValue">The type to convert the fields to for processing.</typeparam>
        public static void ForEachAttributedMember<T, TValue>(this object target, Action<T, TValue> action)
            where T : Attribute =>
            ForEachAttributedMember<T, TValue>(target, (t, _, v) => action(t, v));

        /// <summary>
        /// Runs the action on all fields or properties annotated with <typeparamref name="T"/> with their current
        /// value. Converts the member values for processing.
        /// </summary>
        /// <param name="target">
        /// The receiving object in which to look for fields and properties and for which to run the action.
        /// </param>
        /// <param name="action">The action to run.</param>
        /// <typeparam name="T">The type of the attribute to look for.</typeparam>
        /// <typeparam name="TValue">The type to convert the member values to for processing.</typeparam>
        public static void ForEachAttributedMember<T, TValue>(this object target, Action<T, MemberInfo, TValue> action)
            where T : Attribute
        {
            // Get target and attribute type.
            var type = target.GetType();
            var attributeType = typeof(T);
            var valueType = typeof(TValue);

            // Resolve attribute for fields.
            foreach (var field in type.GetFields())
            {
                // Try to get attributes, run action with collected data and converted field value.
                foreach (var attribute in field.GetCustomAttributes(attributeType))
                    action((T) attribute, field, (TValue) Convert.ChangeType(field.GetValue(target), valueType)!);
            }

            // Resolve attribute for properties.
            foreach (var property in type.GetProperties())
            {
                // Try to get attribute, run action with collected data and converted property value.
                foreach (var attribute in property.GetCustomAttributes(attributeType))
                    action((T) attribute, property, (TValue) Convert.ChangeType(property.GetValue(target), valueType)!);
            }
        }
    }
}