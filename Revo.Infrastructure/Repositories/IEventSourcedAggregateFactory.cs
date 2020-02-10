using System;
using System.Collections.Generic;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.EventStores;

namespace Revo.Infrastructure.Repositories
{
    public interface IEventSourcedAggregateFactory
    {
        IEventSourcedAggregateRoot ConstructAndLoadEntityFromEvents(Guid aggregateId,
            IReadOnlyDictionary<string, string> eventStreamMetadata,
            IReadOnlyCollection<IEventStoreRecord> eventRecords);
    }
}