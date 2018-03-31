using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.ReadModel;
using Revo.Domain.Tenancy;
using Revo.Domain.Tenancy.Events;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// Common stub for EF6 single-read-model entity projections.
    /// </summary>
    public abstract class CrudEntityEventToPocoProjector<TSource, TTarget> :
        EntityEventToPocoProjector<TSource, TTarget>
        where TSource : class, IEventSourcedAggregateRoot
        where TTarget : class, new()
    {
        public CrudEntityEventToPocoProjector(ICrudRepository repository)
        {
            Repository = repository;
        }

        protected ICrudRepository Repository { get; }

        public override Task CommitChangesAsync()
        {
            return Repository.SaveChangesAsync();
        }

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
