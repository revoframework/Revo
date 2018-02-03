using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.Infrastructure.EventStore
{
    public class UncommitedEventStoreRecord : IUncommittedEventStoreRecord
    {
        public UncommitedEventStoreRecord(IEvent @event, IReadOnlyDictionary<string, string> metadata)
        {
            Event = @event;
            Metadata = metadata;
        }

        public IEvent Event { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
