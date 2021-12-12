using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.Domain.ReadModel;
using Revo.Domain.Tenancy.Events;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// A CRUD repository-backed event projector for an aggregate type with a single POCO read model for every aggregate.
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    /// <remarks>
    /// Automatically creates, loads and saves read model instances using the repository.
    /// If TTarget is IManuallyRowVersioned, automatically handles read model versioning and projection
    /// idempotency using event sequence numbers.
    /// Furthermore, automatically sets read model IDs for IEntityReadModel,
    /// class IDs for IClassEntityReadModel and tenant IDs for ITenantReadModel.
    /// </remarks>
    /// <typeparam name="TSource">Aggregate type.</typeparam>
    /// <typeparam name="TTarget">Read model type. Should have a parameterless constructor.</typeparam>
    public abstract class CrudEntityEventToPocoProjector<TSource, TTarget> :
        EntityEventToPocoProjector<TTarget>
        where TSource : class, IAggregateRoot
        where TTarget : class, new()
    {
        public CrudEntityEventToPocoProjector(ICrudRepository repository)
        {
            Repository = repository;
        }

        protected ICrudRepository Repository { get; }

        protected override async Task<TTarget> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
        {
            var rm = await Repository.FindAsync<TTarget>(aggregateId);
            if (rm != null)
            {
                return rm; // in case we previously did a projection, but didn't succeed in dequeuing async events
            }

            rm = new TTarget();

            IClassEntityReadModel classEntityReadModel = rm as IClassEntityReadModel;
            if (classEntityReadModel != null)
            {
                classEntityReadModel.ClassId = events.FirstOrDefault()?.Metadata.GetAggregateClassId() ?? typeof(TSource).GetClassId();
            }

            IEntityReadModel entityReadModel = classEntityReadModel ?? rm as IEntityReadModel;
            if (entityReadModel != null)
            {
                entityReadModel.Id = aggregateId;
            }

            if (rm is ITenantReadModel tenantReadModel)
            {
                var tenantAggregateCreatedEvent =
                    events.OfType<IEventMessage<TenantAggregateRootCreated>>().FirstOrDefault();
                if (tenantAggregateCreatedEvent != null)
                {
                    tenantReadModel.TenantId = tenantAggregateCreatedEvent.Event.TenantId;
                }
                else
                {
                    tenantReadModel.TenantId = events.FirstOrDefault()?.Metadata.GetAggregateTenantId();
                }
            }

            Repository.Add(rm);
            return rm;
        }

        protected override Task<TTarget> GetProjectionTargetAsync(Guid aggregateId)
        {
            return Repository.GetAsync<TTarget>(aggregateId);
        }
    }
}
