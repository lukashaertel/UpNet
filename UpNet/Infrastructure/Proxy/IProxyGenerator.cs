namespace UpNet.Infrastructure.Proxy
{
    public interface IProxyGenerator
    {
        public object?[] GenerateProxyObjects(object?[] objects);
    }
}