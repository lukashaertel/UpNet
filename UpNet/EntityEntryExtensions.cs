using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UpNet
{
    /// <summary>
    /// Extends <see cref="EntityEntry"/>.
    /// </summary>
    public static class EntityEntryExtensions
    {
        /// <summary>
        /// Compiled primary key accessors.
        /// </summary>
        private static Dictionary<IEntityType, Func<EntityEntry, object>> CompiledKeyFunctions { get; } = new();

        /// <summary>
        /// The method info for getting a property.
        /// </summary>
        private static MethodInfo Property { get; } =
            typeof(EntityEntry).GetMethod(nameof(EntityEntry.Property))!;

        /// <summary>
        /// The property info of getting the current value of a property.
        /// </summary>
        private static PropertyInfo CurrentValue { get; } =
            typeof(PropertyEntry).GetProperty(nameof(PropertyEntry.CurrentValue))!;

        /// <summary>
        /// Gets the primary key value for the <see cref="entry"/>.
        /// </summary>
        /// <param name="entry">The entry to get for.</param>
        /// <returns>Returns the array of the primary keys.</returns>
        /// <exception cref="ArgumentException">Thrown if the entry has no primary key.</exception>
        public static object PrimaryKey(this EntityEntry entry)
        {
            // Try return from compiled.
            if (CompiledKeyFunctions.TryGetValue(entry.Metadata, out var func))
                return func(entry);

            // Get names of all primary key properties, should be present.
            var names = entry.Metadata
                            .FindPrimaryKey()
                            .Properties
                            .Select(property => property.Name)
                            .ToList()
                        ?? throw new ArgumentException("Entry type has no primary key");

            // Make lambda for direct result of property access or array initializer.
            var param = Expression.Parameter(typeof(EntityEntry));
            var lambda = Expression.Lambda<Func<EntityEntry, object>>(
                parameters: param,
                body: Expression.NewArrayInit(typeof(object),
                    names.Select(name =>
                        Expression.Property(
                            expression: Expression.Call(param, Property, Expression.Constant(name)),
                            property: CurrentValue))
                ));

            // Compile and update, return result of invocation.
            func = lambda.Compile();
            CompiledKeyFunctions[entry.Metadata] = func;
            return func(entry);
        }
    }
}