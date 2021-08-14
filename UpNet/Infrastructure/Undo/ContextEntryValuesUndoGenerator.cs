using System;
using Microsoft.EntityFrameworkCore;
using UpNet.Extensions;

namespace UpNet.Infrastructure.Undo
{
    /// <summary>
    /// Extends <see cref="DbContext"/> for undo creation.
    /// </summary>
    public sealed class ContextEntryValuesUndoGenerator : IContextUndoGenerator
    {
        public Action<DbContext> CreateUndo(DbContext context)
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
                        result = undoContext =>
                        {
                            capture(undoContext);
                            var target = undoContext.Find(type, keys);
                            undoContext.Remove(target);
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
                        result = undoContext =>
                        {
                            capture(undoContext);
                            var target = Activator.CreateInstance(type)!;
                            var targetEntry = undoContext.Add(target);
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
                        result = undoContext =>
                        {
                            capture(undoContext);
                            var target = undoContext.Find(type, keys);
                            var targetEntry = undoContext.Entry(target);
                            targetEntry.CurrentValues.SetValues(values);
                        };
                        break;
                    }
                }

            return result;
        }
    }
}