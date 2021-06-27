using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace UpNet
{
    /// <summary>
    /// Extends <see cref="InternalEntityEntry"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "EF1001")]
    public static class InternalEntityEntryExtensions
    {
        /// <summary>
        /// Compiled primary key accessors.
        /// </summary>
        private static Dictionary<IEntityType, Func<InternalEntityEntry, object>> CompiledKeyFunctions { get; } = new();

        /// <summary>
        /// The method info for getting the current value of a property.
        /// </summary>
        private static MethodInfo GetCurrentValue { get; } =
            typeof(InternalEntityEntry).GetMethod(
                name: nameof(InternalEntityEntry.GetCurrentValue),
                genericParameterCount: 0,
                types: new[] {typeof(IPropertyBase)})!;

        /// <summary>
        /// Gets the primary key value for the <see cref="entry"/>.
        /// </summary>
        /// <param name="entry">The entry to get for.</param>
        /// <returns>Returns the array of the primary keys.</returns>
        /// <exception cref="ArgumentException">Thrown if the entry has no primary key.</exception>
        public static object PrimaryKey(this InternalEntityEntry entry)
        {
            // Try return from compiled.
            if (CompiledKeyFunctions.TryGetValue(entry.EntityType, out var func))
                return func(entry);

            // Get properties of all primary key properties, should be present.
            var props = entry.EntityType
                            .FindPrimaryKey()
                            .Properties
                            .ToList()
                        ?? throw new ArgumentException("Entry type has no primary key");

            // Make lambda for direct result of current value access or array initializer.
            var param = Expression.Parameter(typeof(InternalEntityEntry));
            var lambda = Expression.Lambda<Func<InternalEntityEntry, object>>(
                parameters: param,
                body: Expression.NewArrayInit(typeof(object),
                    props.Select(prop =>
                        Expression.Call(param, GetCurrentValue, Expression.Constant(prop)))
                ));

            // Compile and update, return result of invocation.
            func = lambda.Compile();
            CompiledKeyFunctions[entry.EntityType] = func;
            return func(entry);
        }
    }
}