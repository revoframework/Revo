using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Events;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.Domain.Events;
using GTRevo.Infrastructure.Domain.EventSourcing;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Transactions;

namespace GTRevo.Infrastructure.Projections
{
    public class ProjectionEventListener : IEventListener<DomainAggregateEvent>,
        IEventQueueTransactionListener
    {
        private readonly IEventSourcedRepository eventSourcedRepository;
        private readonly MultiValueDictionary<Type, IEntityEventProjector> aggregateTypesToEntityEventProjectors = new MultiValueDictionary<Type, IEntityEventProjector>();
        private readonly MultiValueDictionary<Type, ICrudEntityProjector> aggregateTypesToCrudEntityProjectors = new MultiValueDictionary<Type, ICrudEntityProjector>();
        private readonly Dictionary<Guid, PublishedEntityEvents> allEvents = new Dictionary<Guid, PublishedEntityEvents>();
        private readonly IEntityTypeManager entityTypeManager;

        public ProjectionEventListener(IEntityEventProjector[] entityEventProjectors,
            ICrudEntityProjector[] crudEntityProjectors,
            IEventSourcedRepository eventSourcedRepository,
            IEntityTypeManager entityTypeManager)
        {
            this.eventSourcedRepository = eventSourcedRepository;
            this.entityTypeManager = entityTypeManager;

            foreach (var entityEventProjector in entityEventProjectors)
            {
                aggregateTypesToEntityEventProjectors.Add(entityEventProjector.ProjectedAggregateType, entityEventProjector);
            }

            foreach (var crudEntityProjector in crudEntityProjectors)
            {
                throw new NotImplementedException("CRUD entity projectors not implemented yet");
                aggregateTypesToCrudEntityProjectors.Add(crudEntityProjector.ProjectedAggregateType, crudEntityProjector);
            }
        }

        public async Task Handle(DomainAggregateEvent notification)
        {
            PublishedEntityEvents events;
            if (!allEvents.TryGetValue(notification.AggregateId, out events))
            {
                events = new PublishedEntityEvents();
                allEvents[notification.AggregateId] = events;
            }

            events.Add(notification);
        }

        public async Task ExecuteProjectionsAsync()
        {
            var usedProjectors = new HashSet<IEntityEventProjector>();

            foreach (var entityEvents in allEvents)
            {
                var events = entityEvents.Value;
                Guid classId = events.First().AggregateClassId;
                Type entityType = entityTypeManager.GetClrTypeByClassId(classId);

                IEventSourcedAggregateRoot entity = await eventSourcedRepository
                    .GetAsync(entityEvents.Key);

                while (entityType != null)
                {
                    IReadOnlyCollection<IEntityEventProjector> projectors;
                    if (aggregateTypesToEntityEventProjectors.TryGetValue(entityType, out projectors))
                    {
                        foreach (var projector in projectors)
                        {
                            await projector.ProjectEventsAsync(entity, events);
                            usedProjectors.Add(projector);
                        }
                    }

                    entityType = entityType.BaseType;
                }
            }

            foreach (var projector in usedProjectors)
            {
                await projector.CommitChangesAsync();
            }

            allEvents.Clear();
        }

        public void OnTransactionBegin(ITransaction transaction)
        {
        }

        public Task OnTransactionSucceededAsync(ITransaction transaction)
        {
            return ExecuteProjectionsAsync();
        }

        private class PublishedEntityEvents : List<DomainAggregateEvent>
        {
        }
    }
}
