using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UpNet.Infrastructure.Controller;
using UpNet.Infrastructure.Db;
using UpNet.Infrastructure.Messages;
using UpNet.Infrastructure.Proxy;
using UpNet.Infrastructure.Undo;

namespace UpNet.Infrastructure
{
    public sealed class RewindExecutor : IExecutor
    {
        private IServiceProvider ServiceProvider { get; }
        private IContextAccessor ContextAccessor { get; }
        private IMessageRepository MessageRepository { get; }
        private IProxyResolver ProxyResolver { get; }
        private IContextUndoGenerator UndoGenerator { get; }

        public RewindExecutor(IServiceProvider serviceProvider, IContextAccessor contextAccessor,
            IMessageRepository messageRepository, IProxyResolver proxyResolver, IContextUndoGenerator undoGenerator)
        {
            ServiceProvider = serviceProvider;
            ContextAccessor = contextAccessor;
            MessageRepository = messageRepository;
            ProxyResolver = proxyResolver;
            UndoGenerator = undoGenerator;
        }

        private Dictionary<MethodInfo, Type[]> ParameterTypes { get; } = new();

        public async Task ExecuteAll(IEnumerable<Message> messages)
        {
            var set = messages.ToImmutableSortedSet(Message.InstantOnlyComparer);
            var lowest = set.FirstOrDefault();
            if (lowest == null)
                return;

            var context = ContextAccessor.Context;

            foreach (var message in MessageRepository.EnumerateTailDown(lowest.Instant))
            {
                message.Undo(context);
                await context.SaveChangesAsync();
            }

            foreach (var message in set)
                MessageRepository.Insert(message);

            foreach (var message in MessageRepository.EnumerateTailUp(lowest.Instant))
            {
                if (!ParameterTypes.TryGetValue(message.Action, out var types))
                {
                    var parameters = message.Action.GetParameters();
                    ParameterTypes[message.Action] = types = new Type[parameters.Length];
                    for (var i = 0; i < parameters.Length; i++)
                        types[i] = parameters[i].ParameterType;
                }

                // Get target type.
                var controllerType = message.Action.DeclaringType ?? throw new ArgumentException(
                    "Found action without declaring type",
                    nameof(messages));

                //Get target and resolve arguments.
                var instance = ServiceProvider.GetRequiredService(controllerType);
                var arguments = ProxyResolver.ResolveProxyObjects(types, message.ArgumentProxies);

                // Run message.
                message.Action.Invoke(instance, arguments);
                UndoGenerator.CreateUndo(context);
            }
        }
    }
}