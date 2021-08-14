using Microsoft.EntityFrameworkCore;

namespace UpNet.Infrastructure.Db
{
    public interface IContextAccessor
    {
        public DbContext Context { get; }
    }
}