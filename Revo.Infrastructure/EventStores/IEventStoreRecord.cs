using System;
using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.Infrastructure.EventStores
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
