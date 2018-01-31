using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Events.Async;
using GTRevo.Infrastructure.EventSourcing;

namespace GTRevo.Infrastructure.Projections
{
    public abstract class ProjectionEventListener :
        IAsyncEventListener<DomainAggregateEvent>
    {
        private readonly IEventSourcedAggregateRepository eventSourcedRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly Dictionary<Guid, PublishedEntityEvents> allEvents = new Dictionary<Guid, PublishedEntityEvents>();

        public ProjectionEventListener(IEventSourcedAggregateRepository eventSourcedRepository,
            IEntityTypeManager entityTypeManager)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.entityTypeManager = entityTypeManager;
        }

        public abstract IAsyncEventSequencer EventSequencer { get; }

        protected abstract IEnumerable<IEntityEventProjector> GetProjectors(Type entityType);

        public async Task HandleAsync(IEventMessage<DomainAggregateEvent> message, string sequenceName)
        {
            PublishedEntityEvents events;
            if (!allEvents.TryGetValue(message.Event.AggregateId, out events))
            {
                events = new PublishedEntityEvents();
                allEvents[message.Event.AggregateId] = events;
            }

            events.Add(message);
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
                Type entityType = entityTypeManager.GetClrTypeByClassId(classId);

                IEventSourcedAggregateRoot entity = await eventSourcedRepository
                    .GetAsync(entityEvents.Key);

                IEnumerable<IEntityEventProjector> projectors = GetProjectors(entityType);

                foreach (var projector in projectors)
                {
                    await projector.ProjectEventsAsync(entity, events);
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
