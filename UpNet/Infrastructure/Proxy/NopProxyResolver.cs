using System;

namespace UpNet.Infrastructure.Proxy
{
    public class NopProxyResolver : IProxyResolver
    {
        public object?[] ResolveProxyObjects(Type[] types, object?[] objects) =>
            objects;
    }
}