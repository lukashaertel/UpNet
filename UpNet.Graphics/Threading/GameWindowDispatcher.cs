using System;
using System.Collections.Concurrent;
using OpenTK.Windowing.Desktop;

namespace UpNet.Graphics.Threading
{
    /// <summary>
    /// Attaches to a <see cref="GameWindow"/>s events to process actions enqueued with <see cref="Enqueue"/>.
    /// </summary>
    public class GameWindowDispatcher
    {
        /// <summary>
        /// Queue of items to be processed.
        /// </summary>
        private ConcurrentQueue<Action> Queued { get; } = new();

        /// <summary>
        /// Creates the game window dispatcher and attaches it to the <paramref name="target"/>s events.
        /// </summary>
        /// <param name="target">The game window to attach to.</param>
        public GameWindowDispatcher(GameWindow target)
        {
            // Attach to target events.
            target.Load += Process;
            target.Unload += Process;
            target.RenderThreadStarted += Process;
            target.RenderFrame += _ => Process();
            target.UpdateFrame += _ => Process();
        }

        /// <summary>
        /// Processes all queued actions.
        /// </summary>
        public void Process()
        {
            while (Queued.TryDequeue(out var action))
                action();
        }

        /// <summary>
        /// Enqueues an action to be run in any of the game window's events.
        /// </summary>
        /// <param name="action">The action to run.</param>
        public void Enqueue(Action action) =>
            Queued.Enqueue(action);
    }
}