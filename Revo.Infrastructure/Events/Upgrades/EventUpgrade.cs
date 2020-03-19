using System;
using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Events.Upgrades
{
    public abstract class EventUpgrade<TAggregate> : IEventUpgrade
    {
        public IEnumerable<IEventMessage<DomainAggregateEvent>> UpgradeStream(IEnumerable<IEventMessage<DomainAggregateEvent>> eventStream,
            IReadOnlyDictionary<string, string> eventStreamMetadata)
        {
            if (eventStreamMetadata != null
                && eventStreamMetadata.TryGetValue(AggregateEventStreamMetadataNames.ClassId, out string classIdString)
                && Guid.Parse(classIdString) != typeof(TAggregate).GetClassId())
            {
                return eventStream;
            }

            return DoUpgradeStream(eventStream);
        }

        protected abstract IEnumerable<IEventMessage<DomainAggregateEvent>> DoUpgradeStream(
            IEnumerable<IEventMessage<DomainAggregateEvent>> events);
    }
}