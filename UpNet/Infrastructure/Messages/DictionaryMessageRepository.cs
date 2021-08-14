using System;
using System.Collections.Generic;
using UpNet.Data;

namespace UpNet.Infrastructure.Messages
{
    public sealed class DictionaryMessageRepository : IMessageRepository
    {
        // TODO: Sync root or lock acquisition


        private static readonly Message MessageMinimum = new(Instant.MinValue, null!, null!, null!);
        private static readonly Message MessageMaximum = new(Instant.MaxValue, null!, null!, null!);

        private SortedSet<Message> Entries { get; } = new(Message.InstantOnlyComparer);

        public IEnumerable<Message> EnumerateTailDown(Instant instant) =>
            Entries.GetViewBetween(new Message(instant, null!, null!, null!), MessageMaximum).Reverse();

        public IEnumerable<Message> EnumerateTailUp(Instant instant) =>
            Entries.GetViewBetween(new Message(instant, null!, null!, null!), MessageMaximum);

        public void Insert(Message message)
        {
            if (Entries.Contains(message))
                throw new ArgumentException($"Message instant {message.Instant} already used");

            Entries.Add(message);
        }

        public void DropUntil(Instant instant) =>
            Entries.GetViewBetween(MessageMinimum, new Message(instant, null!, null!, null!)).Clear();
    }
}