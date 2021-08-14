using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using UpNet.Extensions;
using UpNet.Infrastructure.Db;

namespace UpNet.Infrastructure.Proxy
{
    // TODO: is this necessary? controllers should be the first responsible to resolve entity.
    public class EntityEnabledProxyGenerator : IProxyGenerator
    {
        private IContextAccessor ContextAccessor { get; }

        public EntityEnabledProxyGenerator(IContextAccessor contextAccessor) =>
            ContextAccessor = contextAccessor;

        [return: NotNullIfNotNull("argument")]
        public object? GenerateProxy(object? argument)
        {
            if (argument == null)
                return null;
            var type = argument.GetType();
            if (type.IsPrimitive)
                return argument;

            if (argument is Array array)
            {
                var result = new object?[array.Length];
                for (var i = 0; i < array.Length; i++)
                    result[i] = GenerateProxy(array.GetValue(i));
                return result;
            }

            if (argument is IList list)
            {
                var result = new List<object?>();
                foreach (var element in list)
                    result.Add(GenerateProxy(element));
                return result;
            }

            if (argument is IDictionary dictionary)
            {
                var result = new Dictionary<object, object?>();
                foreach (var key in dictionary.Keys)
                    result[GenerateProxy(key)] = GenerateProxy(dictionary[key]);
                return result;
            }

            var entityType = ContextAccessor.Context.Model.FindEntityType(type);
            if (null != entityType)
                return ContextAccessor.Context.Entry(argument).PrimaryKey();

            return argument;
        }

        public object?[] GenerateProxyObjects(object?[] objects)
        {
            var result = new object?[objects.Length];
            for (var i = 0; i < objects.Length; i++)
                result[i] = GenerateProxy(objects[i]);
            return result;
        }
    }
}