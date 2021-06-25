using System;
using System.Threading.Tasks;
using OpenTK.Windowing.Desktop;

namespace UpNet.Graphics.Threading
{
    /// <summary>
    /// Extends <see cref="IGLFWGraphicsContext"/>.
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Switches over the context to the a function executed by <see cref="Task.Run(System.Action)"/>. Awaits the
        /// result to switch over properly to the calling thread.
        /// </summary>
        /// <param name="context">The context to run with and switch over.</param>
        /// <param name="func">The function.</param>
        /// <remarks>Operations should in general continue on the captured context.</remarks>
        public static void RunForResult(this IGLFWGraphicsContext context, Func<Task> func)
        {
            // Check if current, make none current if current.
            var isCurrent = context.IsCurrent;
            if (isCurrent)
                context.MakeNoneCurrent();

            try
            {
                // Run new task, get awaiter and get it's result.
                Task.Run(async () =>
                {
                    // Check if somehow already current, if not, make current.
                    var isTaskCurrent = context.IsCurrent;
                    if (!isTaskCurrent)
                        context.MakeCurrent();

                    try
                    {
                        // Run body.
                        await func();
                    }
                    finally
                    {
                        // If was not current, make none current again.
                        if (!isTaskCurrent)
                            context.MakeNoneCurrent();
                    }
                }).GetAwaiter().GetResult();
            }
            finally
            {
                // If was current, make current again.
                if (isCurrent)
                    context.MakeCurrent();
            }
        }

        /// <summary>
        /// Switches over the context to the a function executed by <see cref="Task.Run(System.Action)"/>. Awaits the
        /// result to switch over properly to the calling thread.
        /// </summary>
        /// <param name="context">The context to run with and switch over.</param>
        /// <param name="func">The function.</param>
        /// <returns>Returns the result of <paramref name="func"/>.</returns>
        /// <remarks>Operations should in general continue on the captured context.</remarks>
        public static T RunForResult<T>(this IGLFWGraphicsContext context, Func<Task<T>> func)
        {
            // Check if current, make none current if current.
            var isCurrent = context.IsCurrent;
            if (isCurrent)
                context.MakeNoneCurrent();

            try
            {
                // Run new task, get awaiter and return it's result.
                return Task.Run(async () =>
                {
                    // Check if somehow already current, if not, make current.
                    var isTaskCurrent = context.IsCurrent;
                    if (!isTaskCurrent)
                        context.MakeCurrent();

                    try
                    {
                        // Run body.
                        return await func();
                    }
                    finally
                    {
                        // If was not current, make none current again.
                        if (!isTaskCurrent)
                            context.MakeNoneCurrent();
                    }
                }).GetAwaiter().GetResult();
            }
            finally
            {
                // If was current, make current again.
                if (isCurrent)
                    context.MakeCurrent();
            }
        }
    }
}