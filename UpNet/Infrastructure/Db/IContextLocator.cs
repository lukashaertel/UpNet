using System;

namespace UpNet.Infrastructure.Db
{
    public interface IContextLocator
    {
        public Type PrimaryType { get; }
    }
}