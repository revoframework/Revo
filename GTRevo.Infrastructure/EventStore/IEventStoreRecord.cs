using System;
using System.Collections.Generic;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.EventStore
{
    public interface IEventStoreRecord
    {
        IEvent Event { get; }
        IReadOnlyDictionary<string, string> AdditionalMetadata { get; }
        Guid EventId { get; }
        long StreamSequenceNumber { get; }
        DateTimeOffset StoreDate { get; }
    }
}
