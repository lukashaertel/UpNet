using System;
using System.Threading;
using System.Threading.Tasks;

namespace UpNet
{
    /// <summary>
    /// Lazy with async support. Generator is protected against multiple invocations and will only run on the first
    /// access. Other accesses will return the existing computation.
    /// </summary>
    /// <typeparam name="T">The type of the value to produce.</typeparam>
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// Generator function creating the task providing the result.
        /// </summary>
        private readonly Func<Task<T>> _generator;

        /// <summary>
        /// The runner, assigned on first evaluation.
        /// </summary>
        private Task<T>? _runner;

        /// <summary>   
        /// The lazy state.
        /// <list type="bullet">
        /// <item><term>0: </term><description>Not yet triggered.</description></item>
        /// <item><term>1: </term><description>Generator starting but runner not assigned.</description></item>
        /// <item><term>2: </term><description>Runner assigned, ready.</description></item>
        /// </list>
        /// </summary>
        private int _state;

        /// <summary>
        /// Creates the async lazy with the given generator function.
        /// </summary>
        /// <param name="generator"></param>
        public AsyncLazy(Func<Task<T>> generator) =>
            _generator = generator;

        /// <summary>
        /// Gets or computes the value async. Generation is only triggered on the first invoke.
        /// </summary>
        public Task<T> ValueAsync
        {
            get
            {
                // Move from zero to one, otherwise just read.
                switch (Interlocked.CompareExchange(ref _state, 1, 0))
                {
                    // Moved from zero to one, start generator and set ready.
                    case 0:
                        _runner = _generator();
                        _state = 2;
                        return _runner;

                    // Triggered somewhere else, but not move to runner just yet. Spin a bit, this shouldn't take long.
                    case 1:
                        SpinWait.SpinUntil(() => _state == 2);
                        goto default;

                    // Ready, return runner. Will not be null.
                    default:
                        return _runner!;
                }
            }
        }

        /// <summary>
        /// True if the value is created and completed.
        /// </summary>
        public bool IsValueCreated => _state == 2 && true == _runner?.IsCompleted;
    }
}