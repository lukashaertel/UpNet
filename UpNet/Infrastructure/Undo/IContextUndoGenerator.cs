using System;
using Microsoft.EntityFrameworkCore;

namespace UpNet.Infrastructure.Undo
{
    /// <summary>
    /// Generates undos for a context.
    /// </summary>
    public interface IContextUndoGenerator
    {
        /// <summary>
        /// Analyzes the changes in the <paramref name="context"/> and generates an undo to run to revert those.
        /// </summary>
        /// <param name="context">The context to generate an undo for.</param>
        /// <returns>Returns the action to invoke on a context to revert the actions.</returns>
        public Action<DbContext> CreateUndo(DbContext context);
    }
}