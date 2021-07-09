using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Generators;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using ILogger = Castle.Core.Logging.ILogger;

namespace UpNet
{
    public class Entity
    {
        public int Id { get; set; }

        public List<Entity> RelatesTo { get; set; } = new();

        public float X { get; set; }

        public float Y { get; set; }

        public void MoveBy(float dx, float dy)
        {
            (X, Y) = (X + dx, Y + dy);
            Console.WriteLine($"{Id} moved to ({X},{Y})");
        }

        public override string ToString() => $"{nameof(Id)}: {Id}, {nameof(X)}: {X}, {nameof(Y)}: {Y}";
    }

    public class DBC : DbContext
    {
        public static Action<DbContext> CreateUndo(DbContext context)
        {
            Action<DbContext> result = _ => { };

            foreach (var entry in context.ChangeTracker.Entries())
                switch (entry.State)
                {
                    // Added, remove it.
                    case EntityState.Added:
                    {
                        // Capture relevant values.
                        var type = entry.Metadata.ClrType;
                        var keys = entry.PrimaryKey();
                        var capture = result;

                        // Amend undo.
                        result = context =>
                        {
                            capture(context);
                            var target = context.Find(type, keys);
                            context.Remove(target);
                        };
                        break;
                    }

                    // Removed, restore it.
                    case EntityState.Deleted:
                    {
                        // Capture relevant values.
                        var type = entry.Metadata.ClrType;
                        var capture = result;
                        var values = entry.OriginalValues.Clone();

                        // Amend undo.
                        result = context =>
                        {
                            capture(context);
                            var target = Activator.CreateInstance(type)!;
                            var targetEntry = context.Add(target);
                            targetEntry.CurrentValues.SetValues(values);
                        };
                        break;
                    }

                    // Modified, change it back.
                    case EntityState.Modified:
                    {
                        // Capture relevant values.
                        var type = entry.Metadata.ClrType;
                        var keys = entry.PrimaryKey();
                        var capture = result;
                        var values = entry.OriginalValues.Clone();

                        // Amend undo.
                        result = context =>
                        {
                            capture(context);
                            var target = context.Find(type, keys);
                            var targetEntry = context.Entry(target);
                            targetEntry.CurrentValues.SetValues(values);
                        };
                        break;
                    }
                }

            return result;
        }

        public DbSet<Entity> Entities { get; set; }

        public DBC(DbContextOptions options) : base(options)
        {
        }
    }

    [Serializable]
    public class XInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            Console.WriteLine(
                $"{invocation.TargetType.Name}.{invocation.Method.Name}({string.Join(", ", invocation.Arguments)})");
            invocation.Proceed();
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class FromServicesAttribute : Attribute
    {
        public FromServicesAttribute()
        {
        }
    }

    [Serializable]
    public class SInterceptor : IInterceptor
    {
        public IServiceProvider Provider { get; }

        public SInterceptor(IServiceProvider provider) =>
            Provider = provider;

        public void Intercept(IInvocation invocation)
        {
            var scope = Provider.CreateScope();
            var parameterInfos = invocation.Method.GetParameters();
            for (var i = 0; i < parameterInfos.Length; i++)
            {
                var parameterInfo = parameterInfos[i];
                if (null != parameterInfo.GetCustomAttribute<FromServicesAttribute>())
                    invocation.SetArgumentValue(i, scope.ServiceProvider.GetService(parameterInfo.ParameterType));
            }

            invocation.Proceed();
        }
    }

    public abstract class ExchangeController
    {
        public virtual void DoThing(int x, int y, [FromServices] DBC context = default!)
        {
            context.Entities.Add(new Entity {X = x, Y = y});
            context.SaveChanges();
        }

        public virtual void DoOther([FromServices] DBC context = default!)
        {
            foreach (var entity in context.Entities)
                Console.WriteLine(entity);
        }
    }


    public class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddFilter(l => LogLevel.Trace <= l);
                builder.AddConsole();
            });


            return;
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<DBC>(options => options.UseInMemoryDatabase("main"));

            var serviceProviderFactory = new DefaultServiceProviderFactory();
            var serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);


            var proxyGenerator = new ProxyGenerator();
            var exchangeInterceptor = new XInterceptor();
            var serviceInterceptor = new SInterceptor(serviceProvider);
            var proxy = proxyGenerator.CreateClassProxy<ExchangeController>(
                exchangeInterceptor,
                serviceInterceptor
            );
            proxy.DoThing(2, 5);
            proxy.DoOther();
            return;

            Console.WriteLine("Init");
            using (var scope = serviceProvider.CreateScope())
            using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            {
                var first = new Entity {Id = 1, X = 5f, Y = 0f};
                var second = new Entity {Id = 2, X = 10f, Y = 20f};
                var extra = new Entity {Id = 3, X = 1f, Y = 2f};
                dbc.Entities.Add(first);
                dbc.Entities.Add(second);
                dbc.Entities.Add(extra);

                first.RelatesTo.Add(second);
                second.RelatesTo.Add(first);
                dbc.SaveChanges();
            }

            Console.WriteLine();

            Action<DBC>? sud;

            Console.WriteLine("Change");
            using (var scope = serviceProvider.CreateScope())
            using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            {
                dbc.Entities.Remove(dbc.Entities.Find(3));

                var first = dbc.Entities.Find(1);
                var second = dbc.Entities.Find(2);
                second.X += 5;

                first.RelatesTo.Clear();
                second.RelatesTo.Clear();
                dbc.SaveChanges();

                sud = DBC.CreateUndo(dbc);
            }

            Console.WriteLine("Undoing");
            using (var scope = serviceProvider.CreateScope())
            using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            {
                sud(dbc);
                dbc.SaveChanges();
            }

            Console.WriteLine();
        }
    }
}