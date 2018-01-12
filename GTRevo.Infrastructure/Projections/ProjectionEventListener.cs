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
        private readonly IServiceLocator serviceLocator;
        private readonly Dictionary<Guid, PublishedEntityEvents> allEvents = new Dictionary<Guid, PublishedEntityEvents>();

        public ProjectionEventListener(IEventSourcedAggregateRepository eventSourcedRepository,
            IEntityTypeManager entityTypeManager,
            IServiceLocator serviceLocator)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.entityTypeManager = entityTypeManager;
            this.serviceLocator = serviceLocator;
        }

        public abstract IAsyncEventSequencer EventSequencer { get; }

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

                IEnumerable<IEntityEventProjector> projectors = serviceLocator.GetAll(
                    typeof(IEntityEventProjector<>).MakeGenericType(entityType))
                    .Cast<IEntityEventProjector>();

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
