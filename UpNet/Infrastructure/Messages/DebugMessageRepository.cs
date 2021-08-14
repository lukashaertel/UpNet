using System;
using System.Collections.Generic;
using System.Linq;
using UpNet.Data;

namespace UpNet.Infrastructure.Messages
{
    public class DebugMessageRepository : IMessageRepository
    {
        public IEnumerable<Message> EnumerateTailUp(Instant instant) =>
            Enumerable.Empty<Message>();

        public void Insert(Message message)
        {
            var controller = message.Action.DeclaringType?.Name;
            var name = message.Action.Name;
            var args = string.Join(", ", message.ArgumentProxies);
            Console.WriteLine($"Inserting {controller}.{name}({args}) with undo into repository");
        }

        public void DropUntil(Instant instant)
        {
        }
    }
}