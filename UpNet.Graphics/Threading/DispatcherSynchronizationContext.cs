using System.Threading;

namespace UpNet.Graphics.Threading
{
    // TODO: Verify.
    /// <summary>
    /// Sends and posts to the <see cref="Dispatcher"/> via <see cref="GameWindowDispatcher.Enqueue"/>.
    /// </summary>
    public class DispatcherSynchronizationContext : SynchronizationContext
    {
        /// <summary>
        /// The dispatcher that the callbacks are run on.
        /// </summary>
        private GameWindowDispatcher Dispatcher { get; }

        /// <summary>
        /// Creates the context on on the given <see cref="GameWindowDispatcher"/>.
        /// </summary>
        /// <param name="dispatcher">The dispatcher running the callbacks.</param>
        public DispatcherSynchronizationContext(GameWindowDispatcher dispatcher) =>
            Dispatcher = dispatcher;

        public override void Send(SendOrPostCallback d, object? state) =>
            Dispatcher.Enqueue(() => d(state));

        public override void Post(SendOrPostCallback d, object? state) =>
            Dispatcher.Enqueue(() => d(state));

        public override SynchronizationContext CreateCopy() =>
            new DispatcherSynchronizationContext(Dispatcher);
    }
}