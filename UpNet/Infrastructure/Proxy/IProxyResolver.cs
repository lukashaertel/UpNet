using System;

namespace UpNet.Infrastructure.Proxy
{
    public interface IProxyResolver
    {
        public object?[] ResolveProxyObjects(Type[] types, object?[] objects);
    }
}