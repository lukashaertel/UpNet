using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using UpNet.Data;
using UpNet.Infrastructure;
using UpNet.Infrastructure.Communication;
using UpNet.Infrastructure.Controller;
using UpNet.Infrastructure.Db;
using UpNet.Infrastructure.Messages;
using UpNet.Infrastructure.Proxy;
using UpNet.Infrastructure.Run;
using UpNet.Infrastructure.Undo;

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
        public DbSet<Entity> Entities { get; set; }

        public DBC(DbContextOptions options) : base(options)
        {
        }
    }

    public interface IController
    {
    }

    public class ExchangeController : IController
    {
        public static readonly MethodInfo MethodInfoDoThing =
            typeof(ExchangeController).GetMethod(nameof(DoThing))!;

        public static readonly MethodInfo MethodInfoDoThingWith =
            typeof(ExchangeController).GetMethod(nameof(DoThingWith))!;

        public static readonly MethodInfo MethodInfoDoOther =
            typeof(ExchangeController).GetMethod(nameof(DoOther))!;

        private IRunContext RunContext { get; }
        private DBC Context { get; }

        public ExchangeController(IRunContext runContext, DBC context)
        {
            RunContext = runContext;
            Context = context;
        }

        public void DoThing(int x, int y)
        {
            RunContext.StartAction();

            Context.Entities.Add(new Entity {X = x, Y = y});

            DoOther();

            RunContext.CompleteAction(MethodInfoDoThing, x, y);
            Context.SaveChanges();
        }

        public void DoThingWith(Entity entity)
        {
            RunContext.StartAction();

            var x = entity.X;
            entity.X = entity.Y;
            entity.Y = x;

            RunContext.CompleteAction(MethodInfoDoThingWith, entity);
            Context.SaveChanges();
        }

        public void DoOther()
        {
            RunContext.StartAction();

            foreach (var entity in Context.Entities)
                Console.WriteLine(entity);

            RunContext.CompleteAction(MethodInfoDoOther);
        }
    }

    // public class Host
    // {
    //     private record Message(ushort ActionName, object[] ArgumentProxies, Action<DbContext> Undo);
    //
    //     private SortedDictionary<Instant, Message> Messages { get; } = new();
    //
    //     private Guid Self { get; }
    //
    //     public T GetController<T>()
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    // Controllers
    // User facing resolve -> get activated/connected controller. 
    // Replay facing resolve -> get internal controller.

    public sealed class UpNet
    {
        private Func<object, Task> _publishMessageSequential = _ => Task.CompletedTask;

        private event Func<object, Task>? PublishMessageBacking;

        public event Func<object, Task>? PublishMessage
        {
            add
            {
                PublishMessageBacking += value;
                _publishMessageSequential = PublishMessageBacking.ToSequential<object>();
            }
            remove
            {
                PublishMessageBacking -= value;
                _publishMessageSequential = PublishMessageBacking.ToSequential<object>();
            }
        }


        private Task OnPublishMessage(object arg) =>
            _publishMessageSequential(arg);

        public Task ReceiveMessage()
        {
            return Task.CompletedTask;
        }

        public T GetController<T>() where T : IController
        {
            return default;
        }
    }

    public class Program
    {
        public static event Func<int, Task> AsEn;

        static async Task Main(string[] args)
        {
            var al = new AsyncLazy<int>(() =>
            {
                Console.WriteLine("Generator invoked");
                return Task.FromResult(2);
            });

            AsEn += async i =>
            {
                await Task.Delay(1000);
                Console.WriteLine($"A {i}");
            };
            AsEn += async i =>
            {
                await Task.Delay(500);
                Console.WriteLine($"B {i}");
            };
            var invoker = AsEn.ToSequential<int>();

            Console.WriteLine("Start");
            await invoker(1);
            Console.WriteLine("End");

            Parallel.For(0, 1000, _ => { al.ValueAsync.GetAwaiter().GetResult(); });


            return;
            var un = new UpNet();
            un.PublishMessage += _ =>
            {
                Console.WriteLine("A");
                return Task.CompletedTask;
            };
            un.PublishMessage += _ =>
            {
                Console.WriteLine("B");
                return Task.CompletedTask;
            };

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServiceCollection>(serviceCollection);
            serviceCollection.AddDbContext<DBC>(options => options.UseInMemoryDatabase("main"));
            serviceCollection.AddSingleton<IContextUndoGenerator, ContextEntryValuesUndoGenerator>();
            serviceCollection.AddSingleton<IControllerLocator>(provider =>
            {
                // Get controller interface type and create list of implementations.
                var targetType = typeof(IController);
                var controllerTypes = new List<Type>();

                var collection = provider.GetRequiredService<IServiceCollection>();
                foreach (var descriptor in collection)
                {
                    // Only deal with scoped types.
                    if (ServiceLifetime.Scoped != descriptor.Lifetime)
                        continue;

                    // If implementation is assigned as type and is a controller, add to types.
                    if (descriptor.ImplementationType?.IsAssignableTo(targetType) == true)
                    {
                        controllerTypes.Add(descriptor.ImplementationType);
                        continue;
                    }

                    // If implementation is assigned via factory and is a controller, add return type of controller.
                    if (descriptor.ImplementationFactory?.Method.ReturnType.IsAssignableTo(targetType) == true)
                    {
                        controllerTypes.Add(descriptor.ImplementationFactory.Method.ReturnType);
                        continue;
                    }
                }

                // Return new locator on those types.
                return new ImplementationListControllerLocator(controllerTypes);
            });

            serviceCollection.AddSingleton<ICommunicator, DebugCommunicator>();
            serviceCollection.AddSingleton<IMessageRepository, DictionaryMessageRepository>();
            serviceCollection.AddSingleton<IProxyGenerator, NopProxyGenerator>();
            serviceCollection.AddSingleton<IProxyResolver, NopProxyResolver>();
            serviceCollection.AddSingleton<IContextLocator, SingleContextLocator>();
            serviceCollection.AddSingleton<IContextAccessor, LocatorContextAccessor>();
            serviceCollection.AddSingleton<IExecutor, RewindExecutor>();

            // Add a controller to find it later.
            serviceCollection.AddScoped<IController, ExchangeController>();
            serviceCollection.AddScoped<ExchangeController>();

            var serviceCollectionLocal = new ServiceCollection();
            serviceCollectionLocal.Add(serviceCollection);
            serviceCollectionLocal.AddScoped<IRunContext, SendingRunContext>();

            var serviceCollectionRemote = new ServiceCollection();
            serviceCollectionRemote.Add(serviceCollection);
            serviceCollectionRemote.AddScoped<IRunContext, NopRunContext>();

            var serviceProviderLocal = serviceCollectionLocal.BuildServiceProvider();
            var serviceProviderRemote = serviceCollectionRemote.BuildServiceProvider();

            var executor = serviceProviderRemote.GetRequiredService<IExecutor>();
            executor.ExecuteAll(new[]
            {
                new Message(new Instant(DateTime.MinValue, 0, Guid.Empty),
                    ExchangeController.MethodInfoDoThing, new object[] {1, 2}, null!),
                new Message(new Instant(DateTime.MaxValue, 0, Guid.Empty),
                    ExchangeController.MethodInfoDoThing, new object[] {1, 2}, null!),
            });

            // using var scope = serviceProviderLocal.CreateScope();
            // var context = scope.ServiceProvider.GetRequiredService<DBC>();
            // var first = new Instant(DateTime.Now, 0, Guid.Empty);
            // var second = new Instant(first.At, (byte) (first.Inner + 1), first.Player);
            //
            // context.Entities.Add(new Entity
            // {
            //     Id = 10, X = 20f, Y = 10f
            // });
            // context.SaveChanges();
            //
            // var sendingRunContext = scope.ServiceProvider.GetRequiredService<IRunContext>() as SendingRunContext;
            // var controller = scope.ServiceProvider.GetRequiredService<ExchangeController>();
            //
            // sendingRunContext.RunningAsInstant = first;
            // controller.DoThingWith(context.Entities.First());
            //
            // sendingRunContext.RunningAsInstant = second;
            // controller.DoThingWith(context.Entities.First());
            //
            // var rs = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
            // var td = rs.EnumerateTailDown(first).ToList();
            // var tu = rs.EnumerateTailUp(first).ToList();
            // var eqr = td.AsEnumerable().Reverse().ToList().SequenceEqual(tu);
            Console.WriteLine();
            // proxy.DoThing(2, 5);
            // proxy.DoOther();
            // return;
            //
            // Console.WriteLine("Init");
            // using (var scope = serviceProvider.CreateScope())
            // using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            // {
            //     var first = new Entity {Id = 1, X = 5f, Y = 0f};
            //     var second = new Entity {Id = 2, X = 10f, Y = 20f};
            //     var extra = new Entity {Id = 3, X = 1f, Y = 2f};
            //     dbc.Entities.Add(first);
            //     dbc.Entities.Add(second);
            //     dbc.Entities.Add(extra);
            //
            //     first.RelatesTo.Add(second);
            //     second.RelatesTo.Add(first);
            //     dbc.SaveChanges();
            // }
            //
            // Console.WriteLine();
            //
            // Action<DBC>? sud;
            //
            // Console.WriteLine("Change");
            // using (var scope = serviceProvider.CreateScope())
            // using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            // {
            //     dbc.Entities.Remove(dbc.Entities.Find(3));
            //
            //     var first = dbc.Entities.Find(1);
            //     var second = dbc.Entities.Find(2);
            //     second.X += 5;
            //
            //     first.RelatesTo.Clear();
            //     second.RelatesTo.Clear();
            //     dbc.SaveChanges();
            //
            //     sud = ContextEntryValuesUndoGenerator.CreateUndo(dbc);
            // }
            //
            // Console.WriteLine("Undoing");
            // using (var scope = serviceProvider.CreateScope())
            // using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            // {
            //     sud(dbc);
            //     dbc.SaveChanges();
            // }
            //
            // Console.WriteLine();
        }
    }
}