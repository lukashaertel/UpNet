using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace UpNet.Infrastructure.Db
{
    public class LocatorContextAccessor : IContextAccessor
    {
        public LocatorContextAccessor(IContextLocator locator, IServiceProvider serviceProvider) =>
            Context = (DbContext) serviceProvider.GetRequiredService(locator.PrimaryType);

        public DbContext Context { get; }
    }
}