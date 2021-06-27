using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace UpNet
{
    class Entity
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

    class CollectingNotifier : IInternalEntityEntryNotifier
    {
        public static ConcurrentDictionary<
            (InternalEntityEntry Entry, INavigation Navigation),
            (object OldValue, object NewValue)> NavigationReferenceChanges { get; } = new();

        public static ConcurrentDictionary<
            (InternalEntityEntry Entry, INavigationBase Navigation),
            (IEnumerable<object> Added, IEnumerable<object> Removed)> NavigationCollectionChanges { get; } = new();

        private IInternalEntityEntryNotifier DelegateTo { get; }

        public CollectingNotifier(
            ILocalViewListener localViewListener,
            IChangeDetector changeDetector,
            INavigationFixer navigationFixer) =>
            DelegateTo = new InternalEntityEntryNotifier(
                localViewListener,
                changeDetector,
                navigationFixer);

        public void StateChanging(InternalEntityEntry entry, EntityState newState)
        {
            Console.WriteLine($"State changing");
            DelegateTo.StateChanging(entry, newState);
        }

        public void StateChanged(InternalEntityEntry entry, EntityState oldState, bool fromQuery)
        {
            Console.WriteLine($"State changed");
            DelegateTo.StateChanged(entry, oldState, fromQuery);
        }

        public void TrackedFromQuery(InternalEntityEntry entry)
        {
            DelegateTo.TrackedFromQuery(entry);
        }

        public void NavigationReferenceChanged(InternalEntityEntry entry, INavigation navigation, object oldValue,
            object newValue)
        {
            //todo: aggregate, could be multiple changes.
            NavigationReferenceChanges[(entry, navigation)] = (oldValue, newValue);
            DelegateTo.NavigationReferenceChanged(entry, navigation, oldValue, newValue);
        }

        public void NavigationCollectionChanged(InternalEntityEntry entry, INavigationBase navigationBase,
            IEnumerable<object> added,
            IEnumerable<object> removed)
        {
            //todo: aggregate, could be multiple changes.
            NavigationCollectionChanges[(entry, navigationBase)] = (added, removed);
            DelegateTo.NavigationCollectionChanged(entry, navigationBase, added, removed);
        }

        public void KeyPropertyChanged(InternalEntityEntry entry, IProperty property, IEnumerable<IKey> keys,
            IEnumerable<IForeignKey> foreignKeys,
            object oldValue, object newValue)
        {
            DelegateTo.KeyPropertyChanged(entry, property, keys, foreignKeys, oldValue, newValue);
        }

        public void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModified)
        {
            Console.WriteLine($"Property {property.Name} changing");
            DelegateTo.PropertyChanged(entry, property, setModified);
        }

        public void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
        {
            Console.WriteLine($"Property {property.Name} changed");
            DelegateTo.PropertyChanging(entry, property);
        }
    }

    class DBC : DbContext
    {
        public DbSet<Entity> Entities { get; set; }

        public DBC(DbContextOptions options) : base(options)
        {
            // base.ChangeTracker.Tracked += ExternalizeNavigations;
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }

        // TODO: Save relationship snapshots and allow access.
        // public override ChangeTracker ChangeTracker =>
        //     throw new InvalidOperationException("Enumerating entries invalidates relationship snapshots");
        //
        // protected ChangeTracker UnsafeChangeTracker =>
        //     base.ChangeTracker;

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var navReferenceChanges = CollectingNotifier.NavigationReferenceChanges.ToList();
            CollectingNotifier.NavigationReferenceChanges.Clear();

            var navCollectionChanges = CollectingNotifier.NavigationCollectionChanges.ToList();
            CollectingNotifier.NavigationCollectionChanges.Clear();
            Console.WriteLine();
            // var cd = (IDbContextDependencies) this;
            //
            // var reset = new Action<DbContext>(_ => { });
            //
            // foreach (var internalEntityEntry in cd.StateManager.Entries)
            // {
            //     if (internalEntityEntry.EntityState == EntityState.Unchanged)
            //         continue;
            //
            //     var key = internalEntityEntry.PrimaryKey();
            //     foreach (var property in internalEntityEntry.EntityType.GetProperties())
            //     {
            //         var ov = internalEntityEntry.GetOriginalValue(property);
            //         var cv = internalEntityEntry.GetCurrentValue(property);
            //
            //
            //         Console.Write(key);
            //         Console.Write(".");
            //         Console.Write(property.Name);
            //         Console.Write(": ");
            //         Console.Write($"{ov}");
            //         Console.Write(" -> ");
            //         Console.Write($"{cv}");
            //         Console.WriteLine();
            //     }
            //
            //     foreach (var navigation in internalEntityEntry.EntityType.GetNavigations())
            //     {
            //         var ov = internalEntityEntry.GetRelationshipSnapshotValue(navigation);
            //         var cv = internalEntityEntry.GetCurrentValue(navigation);
            //         if (ov is not IEnumerable<object> ove) continue;
            //         if (cv is not IEnumerable<object> cve) continue;
            //
            //         ove = ove.Select(o => cd.StateManager.TryGetEntry(o, navigation.TargetEntityType).PrimaryKey())
            //             .ToList();
            //         cve = cve.Select(o => cd.StateManager.TryGetEntry(o, navigation.TargetEntityType).PrimaryKey())
            //             .ToList();
            //
            //         Console.Write(key);
            //         Console.Write(".");
            //         Console.Write(navigation.Name);
            //         Console.Write(": ");
            //         Console.Write($"[{string.Join(",", ove)}]");
            //         Console.Write(" -> ");
            //         Console.Write($"[{string.Join(",", cve)}]");
            //         Console.WriteLine();
            //     }
            // }
            //
            //
            // // // TODO: All
            // // foreach (var entry in ChangeTracker.Entries().ToList())
            // // {
            // //     foreach (var property in entry.Navigations)
            // //     {
            // //         var before = OriginalValues.GetValueOrDefault((entry.Entity, property.Metadata));
            // //         var now = property.CurrentValue;
            // //
            // //         Console.WriteLine(
            // //             $"Change: {entry.Entity}.{property.Metadata.Name}: {ValueToString(before)} to {ValueToString(now)}");
            // //     }
            // // }
            //

            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<DBC>(options =>
            {
                options.UseInMemoryDatabase("main");
                options.ReplaceService<IInternalEntityEntryNotifier,
                    InternalEntityEntryNotifier,
                    CollectingNotifier>();
            });

            var serviceProviderFactory = new DefaultServiceProviderFactory();
            var serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);

            Console.WriteLine("Init");
            using (var scope = serviceProvider.CreateScope())
            using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            {
                var first = new Entity {X = 5f, Y = 0f};
                var second = new Entity {X = 10f, Y = 20f};
                dbc.Entities.Add(first);
                dbc.Entities.Add(second);

                first.RelatesTo.Add(second);
                second.RelatesTo.Add(first);
                dbc.SaveChanges();
            }

            Console.WriteLine();

            Console.WriteLine("Change");
            using (var scope = serviceProvider.CreateScope())
            using (var dbc = scope.ServiceProvider.GetRequiredService<DBC>())
            {
                var first = dbc.Entities.First();
                var second = dbc.Entities.Last();
                Console.WriteLine("Setting x");
                second.X += 5;
                Console.WriteLine("Set x");

                first.RelatesTo.Clear();
                second.RelatesTo.Clear();
                dbc.SaveChanges();
            }

            Console.WriteLine();
        }
    }
}