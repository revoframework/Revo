using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventSourcing;

namespace Revo.Infrastructure.Projections
{
    public abstract class ProjectionEventListener :
        IAsyncEventListener<DomainAggregateEvent>
    {
        private readonly IEntityTypeManager entityTypeManager;
        private readonly Dictionary<Guid, PublishedEntityEvents> allEvents = new Dictionary<Guid, PublishedEntityEvents>();

        public ProjectionEventListener(IEntityTypeManager entityTypeManager)
        {
            this.entityTypeManager = entityTypeManager;
        }

        public abstract IAsyncEventSequencer EventSequencer { get; }

        public abstract IEnumerable<IEntityEventProjector> GetProjectors(Type entityType);

        public Task HandleAsync(IEventMessage<DomainAggregateEvent> message, string sequenceName)
        {
            PublishedEntityEvents events;
            if (!allEvents.TryGetValue(message.Event.AggregateId, out events))
            {
                events = new PublishedEntityEvents();
                allEvents[message.Event.AggregateId] = events;
            }

            events.Add(message);
            return Task.FromResult(0);
        }

        public Task OnFinishedEventQueueAsync(string sequenceName)
        {
            return ExecuteProjectionsAsync();
        }

        public async Task ExecuteProjectionsAsync()
        {
            var usedProjectors = new HashSet<IEntityEventProjector>();

            foreach (var entityEvents in allEvents)
            {
                var events = entityEvents.Value;
                Guid classId = events.First().Metadata.GetAggregateClassId() ?? throw new InvalidOperationException($"Cannot create projection for aggregate ID {entityEvents.Key} because event metadata don't contain aggregate class ID");
                Type entityType = entityTypeManager.GetClassInfoByClassId(classId).ClrType;

                Guid aggregateId = entityEvents.Key;
                IEnumerable<IEntityEventProjector> projectors = GetProjectors(entityType);

                foreach (var projector in projectors)
                {
                    await projector.ProjectEventsAsync(aggregateId, events);
                    usedProjectors.Add(projector);
                }
            }

            foreach (var projector in usedProjectors)
            {
                await projector.CommitChangesAsync();
            }

            allEvents.Clear();
        }
        
        private class PublishedEntityEvents : List<IEventMessage<DomainAggregateEvent>>
        {
        }
    }
}
