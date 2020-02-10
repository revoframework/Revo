using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Upgrades;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStores;

namespace Revo.Infrastructure.Repositories
{
    public class EventSourcedAggregateFactory : IEventSourcedAggregateFactory
    {
        private readonly IEventStreamUpgrader eventStreamUpgrader;
        private readonly IEntityFactory entityFactory;
        private readonly IEntityTypeManager entityTypeManager;

        public EventSourcedAggregateFactory(IEventStreamUpgrader eventStreamUpgrader, IEntityFactory entityFactory, IEntityTypeManager entityTypeManager)
        {
            this.eventStreamUpgrader = eventStreamUpgrader;
            this.entityFactory = entityFactory;
            this.entityTypeManager = entityTypeManager;
        }


        public IEventSourcedAggregateRoot ConstructAndLoadEntityFromEvents(Guid aggregateId, IReadOnlyDictionary<string, string> eventStreamMetadata,
            IReadOnlyCollection<IEventStoreRecord> eventRecords)
        {
            if (!eventStreamMetadata.TryGetValue(AggregateEventStreamMetadataNames.ClassId, out string classIdString))
            {
                throw new InvalidOperationException($"Cannot load event sourced aggregate ID {aggregateId}: aggregate class ID not found in event stream metadata");
            }

            Guid classId = Guid.Parse(classIdString);
            Type entityType = entityTypeManager.GetClassInfoByClassId(classId).ClrType;

            IEnumerable<IEventMessage<DomainAggregateEvent>> eventMessages;
            try
            {
                eventMessages = eventRecords.Select(EventStoreEventMessage.FromRecord)
                    .Cast<IEventMessage<DomainAggregateEvent>>();
            }
            catch (InvalidCastException e)
            {
                throw new InvalidOperationException(
                    $"Cannot load event sourced aggregate ID {aggregateId}: event stream contains non-DomainAggregateEvent events",
                    e);
            }

            var upgradedEvents = eventStreamUpgrader.UpgradeStream(eventMessages, eventStreamMetadata);
            var events = upgradedEvents.Select(x => x.Event).ToArray();

            int version = (int)(eventRecords.LastOrDefault()?.AdditionalMetadata.GetAggregateVersion() // use non-upgraded event records to preserve the versions
                                 ?? eventRecords.LastOrDefault()?.StreamSequenceNumber
                                 ?? 0);

            AggregateState state = new AggregateState(version, events);

            IEventSourcedAggregateRoot aggregate = (IEventSourcedAggregateRoot) ConstructEntity(entityType, aggregateId);
            aggregate.LoadState(state);

            return aggregate;
        }

        private IEntity ConstructEntity(Type entityType, Guid id)
        {
            return entityFactory.ConstructEntity(entityType, id);
        }
    }
}