using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace UpNet.Graphics.Threading
{
    /// <summary>
    /// Extends <see cref="GameWindowDispatcher"/>.
    /// </summary>
    public static class GameWindowDispatcherExtensions
    {
        /// <summary>
        /// Enqueues an action with the dispatcher and awaits it's completion.
        /// </summary>
        /// <param name="dispatcher">The dispatcher to run on.</param>
        /// <param name="action">The action to run.</param>
        /// <returns>Returns a task that's completed when the dispatcher processed the action.</returns>
        public static Task EnqueueAsync(this GameWindowDispatcher dispatcher, Action action)
        {
            // Completion source to activate when processed.
            var completionSource = new TaskCompletionSource();

            // Dispatch the action wrapping completion.
            dispatcher.Enqueue(() =>
            {
                try
                {
                    // Run the action and complete.
                    action();
                    completionSource.SetResult();
                }
                catch (Exception exception)
                {
                    // Failed, transmit the exception.
                    completionSource.SetException(exception);
                }
            });

            // Return the completion source's task.
            return completionSource.Task;
        }

        /// <summary>
        /// Enqueues a function with the dispatcher and awaits it's completion.
        /// </summary>
        /// <param name="dispatcher">The dispatcher to run on.</param>
        /// <param name="action">The function to run for it's result.</param>
        /// <returns>
        /// Returns a task that's completed with the function's result when the dispatcher processed the action.
        /// </returns>
        public static Task<T> EnqueueAsync<T>(this GameWindowDispatcher dispatcher, Func<T> action)
        {
            // Completion source to activate when processed.
            var completionSource = new TaskCompletionSource<T>();

            // Dispatch the action wrapping completion.
            dispatcher.Enqueue(() =>
            {
                try
                {
                    // Run the function and complete with it's result.
                    completionSource.SetResult(action());
                }
                catch (Exception exception)
                {
                    // Failed, transmit the exception.
                    completionSource.SetException(exception);
                }
            });

            // Return the completion source's task.
            return completionSource.Task;
        }

        /// <summary>
        /// Actively waits for the task to complete, running <see cref="GameWindowDispatcher.Process"/> while waiting
        /// for <paramref name="task"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher that should process events while waiting.</param>
        /// <param name="task">The task to wait for.</param>
        /// <param name="millisecondsTaskWaitTimeout">The wait time after processed once.</param>
        public static void ActiveGetResult(this GameWindowDispatcher dispatcher, Task task,
            int millisecondsTaskWaitTimeout = 5)
        {
            // While not completed, process once then wait for the task.
            while (!task.IsCompleted)
            {
                dispatcher.Process();
                if (task.Wait(millisecondsTaskWaitTimeout))
                    break;
            }

            // Unroll task.
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Actively waits for the task to complete, running <see cref="GameWindowDispatcher.Process"/> while waiting
        /// for <paramref name="task"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher that should process events while waiting.</param>
        /// <param name="task">The task to wait for.</param>
        /// <param name="millisecondsTaskWaitTimeout">The wait time after processed once.</param>
        /// <typeparam name="T">The type of the task's result.</typeparam>
        /// <returns>Returns the task's result.</returns>
        public static T ActiveGetResult<T>(this GameWindowDispatcher dispatcher, Task<T> task,
            int millisecondsTaskWaitTimeout = 5)
        {
            // While not completed, process once then wait for the task.
            while (!task.IsCompleted)
            {
                dispatcher.Process();
                if (task.Wait(millisecondsTaskWaitTimeout))
                    break;
            }

            // Unroll task, return the result.
            return task.GetAwaiter().GetResult();
        }
    }
}