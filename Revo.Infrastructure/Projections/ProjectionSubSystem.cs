using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Projections
{
    public abstract class ProjectionSubSystem : IProjectionSubSystem
    {
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IEventMessageFactory eventMessageFactory;

        protected ProjectionSubSystem(IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory)
        {
            this.entityTypeManager = entityTypeManager;
            this.eventMessageFactory = eventMessageFactory;
        }

        public virtual async Task ExecuteProjectionsAsync(IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events,
            IUnitOfWork unitOfWork, EventProjectionOptions options)
        {
            var usedProjectors = new HashSet<IEntityEventProjector>();
            var aggregateEvents = events.GroupBy(x => x.Event.AggregateId);

            foreach (var entityEvents in aggregateEvents)
            {
                Guid classId = entityEvents.First().Metadata.GetAggregateClassId()
                               ?? throw new InvalidOperationException($"Cannot create projection for aggregate ID {entityEvents.Key} because event metadata don't contain aggregate class ID");
                Type entityType = entityTypeManager.GetClassInfoByClassId(classId).ClrType;

                Guid aggregateId = entityEvents.Key;
                IEnumerable<IEntityEventProjector> projectors = GetProjectors(entityType, options);

                foreach (var projector in projectors)
                {
                    if (projector is IEventPublishingEntityEventProjector publishingProjector)
                    {
                        publishingProjector.EventBuffer = unitOfWork?.EventBuffer;
                        publishingProjector.EventMessageFactory = eventMessageFactory;
                    }

                    await projector.ProjectEventsAsync(aggregateId, entityEvents.ToArray());
                    usedProjectors.Add(projector);
                }
            }

            await CommitUsedProjectorsAsync(usedProjectors, options);
        }

        protected abstract IEnumerable<IEntityEventProjector> GetProjectors(Type entityType, EventProjectionOptions options);

        protected virtual async Task CommitUsedProjectorsAsync(IReadOnlyCollection<IEntityEventProjector> usedProjectors, EventProjectionOptions options)
        {
            foreach (var projector in usedProjectors)
            {
                await projector.CommitChangesAsync();
            }
        }
    }
}
