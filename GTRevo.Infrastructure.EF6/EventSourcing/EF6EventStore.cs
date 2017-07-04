using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core;
using GTRevo.DataAccess.EF6.Entities;
using GTRevo.Infrastructure.Domain.Events;
using GTRevo.Infrastructure.Domain.EventSourcing;
using GTRevo.Infrastructure.EF6.EventSourcing.Model;
using GTRevo.Infrastructure.EventSourcing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GTRevo.Infrastructure.EF6.EventSourcing
{
    public class EF6EventStore : IEventStore
    {
        private readonly ICrudRepository repository;
        private readonly DomainEventTypeCache domainEventTypeCache;

        public EF6EventStore(ICrudRepository repository,
            DomainEventTypeCache domainEventTypeCache)
        {
            this.repository = repository;
            this.domainEventTypeCache = domainEventTypeCache;
        }

        public Task<AggregateState> GetStateByVersionAsync(Guid aggregateId, int version)
        {
            throw new NotImplementedException();
        }

        public async Task<IAggregateHistory> GetAggregateHistory(Guid aggregateId)
        {
            // TODO might cache previously loaded aggregate (if any) and use them
            // to optimize this query
            var record = await repository.GetAsync<DomainAggregateRecord>(aggregateId);
            var lastPacket = await QueryAggregateEventPackets(aggregateId, true)
                .FirstAsync();

            return new AggregateHistory(record.Id, record.ClassId,
                lastPacket.SequenceNumber, () => GetAggregateEventRecords(aggregateId));
        }

        public async Task<AggregateState> GetLastStateAsync(Guid aggregateId)
        {
            var packets = await QueryAggregateEventPackets(aggregateId)
                .ToListAsync();

            if (packets.Count == 0)
            {
                throw new ArgumentException($"No source events found for aggregate ID '{aggregateId}'");
            }

            var events = packets
                .SelectMany(SelectEventRecordsFromPackets)
                .Select(SelectEventFromRecord);

            bool isDeleted = false; //events.Any(x => x is AggregateDeleted)

            AggregateState state = new AggregateState(packets.Last().SequenceNumber, events, isDeleted);
            return state;
        }

        private async Task<IEnumerable<DomainAggregateEventRecord>> GetAggregateEventRecords(
            Guid aggregateId)
        {
            var packets = await QueryAggregateEventPackets(aggregateId)
                .ToListAsync();
            return packets.SelectMany(SelectEventRecordsFromPackets);
        }

        private IQueryable<DomainEventPacket> QueryAggregateEventPackets(
            Guid aggregateId, bool reverseOrder = false)
        {
            var packets = repository.FindAll<DomainEventPacket>()
                .Where(x => x.AggregateId == aggregateId);

            if (!reverseOrder)
            {
                packets = packets.OrderBy(x => x.SequenceNumber);
            }
            else
            {
                packets = packets.OrderByDescending(x => x.SequenceNumber);
            }

            return packets;
        }

        private IEnumerable<DomainAggregateEventRecord> SelectEventRecordsFromPackets(
            DomainEventPacket packet)
        {
            JArray eventsArray = JArray.Parse(packet.EventsJson);
            foreach (JObject eventDescriptor in eventsArray)
            {
                DomainEventSerialized eventSerialized = eventDescriptor.ToObject<DomainEventSerialized>();
                JObject eventObject = eventSerialized.Data;

                //TODO: optimize
                Type eventType = domainEventTypeCache.GetClrEventType(
                    eventSerialized.EventName, eventSerialized.EventVersion);

                if (eventType == null)
                {
                    throw new ArgumentException($"Domain event type not found: {eventSerialized.EventName} v. {eventSerialized.EventVersion}");
                }

                if (!typeof(DomainAggregateEvent).IsAssignableFrom(eventType))
                {
                    throw new ArgumentException($"Invalid domain aggregate event type: {eventType.FullName}");
                }

                DomainAggregateEvent domainEvent = (DomainAggregateEvent)eventObject.ToObject(eventType);

                DomainAggregateEventRecord record = new DomainAggregateEventRecord()
                {
                    ActorName = packet.ActorName,
                    AggregateVersion = packet.SequenceNumber,
                    DatePublished = packet.DatePublished,
                    Event = domainEvent
                };

                yield return record;
            }
        }

        private DomainAggregateEvent SelectEventFromRecord(DomainAggregateEventRecord record)
        {
            return record.Event;
        }

        public void AddAggregate(Guid aggregateId, Guid aggregateClassId)
        {
            DomainAggregateRecord aggregateRecord = new DomainAggregateRecord()
            {
                Id = aggregateId,
                ClassId = aggregateClassId
            };

            repository.Add(aggregateRecord);
        }

        public void PushEvents(Guid aggregateId, IEnumerable<DomainAggregateEventRecord> events, int version)
        {
            JArray eventsArray = new JArray();
            foreach (DomainAggregateEventRecord domainEventRecord in events)
            {
                var nameAndVersion = domainEventTypeCache.GetEventNameAndVersion(domainEventRecord.Event.GetType());

                DomainEventSerialized eventSerialized = new DomainEventSerialized();
                eventSerialized.Data = JObject.FromObject(domainEventRecord.Event);
                eventSerialized.EventName = nameAndVersion.Item1;
                eventSerialized.EventVersion = nameAndVersion.Item2;

                eventsArray.Add(JObject.FromObject(eventSerialized));
            }

            string eventsJson = eventsArray.ToString(Formatting.None);

            DomainEventPacket packet = new DomainEventPacket()
            {
                Id = Guid.NewGuid(),
                AggregateId = aggregateId,
                SequenceNumber = version,
                ActorName = events.LastOrDefault()?.ActorName,
                DatePublished = Clock.Current.Now,
                EventsJson = eventsJson,
            };

            repository.Add(packet);
        }

        public Task PushEventsAsync(Guid aggregateId, IEnumerable<DomainAggregateEventRecord> events, int version)
        {
            PushEvents(aggregateId, events, version);
            return Task.FromResult(0);
        }

        public void CommitChanges()
        {
            repository.SaveChanges();
        }

        public Task CommitChangesAsync()
        {
            return repository.SaveChangesAsync();
        }
    }
}
