using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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

    class DBC : DbContext
    {
        // private static object? ExternalizeValue(object? value) =>
        //     value is IEnumerable enumerable
        //         ? enumerable.Cast<object>().ToList()
        //         : value;
        //
        // private static string? ValueToString(object? value) =>
        //     value is IEnumerable enumerable
        //         ? "[" + string.Join(", ", enumerable.Cast<object>()) + "]"
        //         : value?.ToString();
        //
        public DbSet<Entity> Entities { get; set; }

        //
        // private Dictionary<(object, INavigationBase), object?> OriginalValues { get; } = new();
        //
        public DBC(DbContextOptions options) : base(options)
        {
            // base.ChangeTracker.Tracked += ExternalizeNavigations;
        }
        //
        //
        // private void ExternalizeNavigations(object? sender, EntityTrackedEventArgs e)
        // {
        //     foreach (var navigation in e.Entry.Navigations)
        //     {
        //         navigation.Load();
        //         Console.WriteLine(
        //             $"Tracking: {e.Entry.Entity}.{navigation.Metadata.Name} at {ValueToString(navigation.CurrentValue)}");
        //         OriginalValues[(e.Entry.Entity, navigation.Metadata)] = ExternalizeValue(navigation.CurrentValue);
        //     }
        // }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var cd = (IDbContextDependencies) this;

            object GetEntityId(object entity, IEntityType entityType)
            {
                var entry = cd.StateManager.TryGetEntry(entity, entityType);

                var key = entityType.GetKeys().First();
                var keyValues = new object[key.Properties.Count];
                for (int i = 0; i < key.Properties.Count; i++)
                {
                    var property = entityType.GetProperty(key.Properties[i].Name);
                    keyValues[i] = entry.GetCurrentValue(property);
                }

                return keyValues.SingleOrDefault() ?? keyValues;
            }

            foreach (var internalEntityEntry in cd.StateManager.Entries)
            {
                if (internalEntityEntry.EntityState == EntityState.Unchanged)
                    continue;

                var key = GetEntityId(internalEntityEntry.Entity, internalEntityEntry.EntityType);
                foreach (var property in internalEntityEntry.EntityType.GetProperties())
                {
                    var ov = internalEntityEntry.GetOriginalValue(property);
                    var cv = internalEntityEntry.GetCurrentValue(property);

                    Console.Write(key);
                    Console.Write(".");
                    Console.Write(property.Name);
                    Console.Write(": ");
                    Console.Write($"{ov}");
                    Console.Write(" -> ");
                    Console.Write($"{cv}");
                    Console.WriteLine();
                }

                foreach (var navigation in internalEntityEntry.EntityType.GetNavigations())
                {
                    var ov = internalEntityEntry.GetRelationshipSnapshotValue(navigation);
                    var cv = internalEntityEntry.GetCurrentValue(navigation);
                    if (ov is not IEnumerable<object> ove) continue;
                    if (cv is not IEnumerable<object> cve) continue;

                    ove = ove.Select(o => GetEntityId(o, navigation.TargetEntityType)).ToList();
                    cve = cve.Select(o => GetEntityId(o, navigation.TargetEntityType)).ToList();

                    Console.Write(key);
                    Console.Write(".");
                    Console.Write(navigation.Name);
                    Console.Write(": ");
                    Console.Write($"[{string.Join(",", ove)}]");
                    Console.Write(" -> ");
                    Console.Write($"[{string.Join(",", cve)}]");
                    Console.WriteLine();
                }
            }


            // // TODO: All
            // foreach (var entry in ChangeTracker.Entries().ToList())
            // {
            //     foreach (var property in entry.Navigations)
            //     {
            //         var before = OriginalValues.GetValueOrDefault((entry.Entity, property.Metadata));
            //         var now = property.CurrentValue;
            //
            //         Console.WriteLine(
            //             $"Change: {entry.Entity}.{property.Metadata.Name}: {ValueToString(before)} to {ValueToString(now)}");
            //     }
            // }


            return base.SaveChanges(acceptAllChangesOnSuccess);
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<DBC>(options => { options.UseInMemoryDatabase("main"); });

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
                second.X += 5;

                first.RelatesTo.Clear();
                second.RelatesTo.Clear();
                dbc.SaveChanges();
            }

            Console.WriteLine();
        }
    }
}