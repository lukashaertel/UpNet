using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UpNet.Infrastructure.Db
{
    public class SingleContextLocator : IContextLocator
    {
        public Type PrimaryType { get; }

        public SingleContextLocator(IServiceCollection serviceCollection)
        {
            var type = serviceCollection.SingleOrDefault(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Scoped &&
                descriptor.ServiceType.IsAssignableTo(typeof(DbContext)));

            if (type == null)
                throw new ArgumentException("Not a single service for DbContext in collection",
                    nameof(serviceCollection));

            PrimaryType = type.ServiceType;
        }
    }
}