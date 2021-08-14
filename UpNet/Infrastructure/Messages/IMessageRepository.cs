using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UpNet.Data;

namespace UpNet.Infrastructure.Messages
{
    public record Message(Instant Instant, MethodInfo Action, object?[] ArgumentProxies, Action<DbContext> Undo)
    {
        private sealed class InstantOnlyComparerImplementation : IComparer<Message>
        {
            public int Compare(Message? x, Message? y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(null, y)) return 1;
                if (ReferenceEquals(null, x)) return -1;
                return x.Instant.CompareTo(y.Instant);
            }
        }

        public static readonly IComparer<Message> InstantOnlyComparer = new InstantOnlyComparerImplementation();
    }

    public interface IMessageRepository
    {
        public IEnumerable<Message> EnumerateTailDown(Instant instant) =>
            EnumerateTailUp(instant).Reverse();

        public IEnumerable<Message> EnumerateTailUp(Instant instant);

        public void Insert(Message message);

        public void DropUntil(Instant instant);
    }
}