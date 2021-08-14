namespace UpNet.Infrastructure.Proxy
{
    /// <summary>
    /// Does not do actual proxyfication.
    /// </summary>
    public class NopProxyGenerator : IProxyGenerator
    {
        public object?[] GenerateProxyObjects(object?[] objects) =>
            objects;
    }
}