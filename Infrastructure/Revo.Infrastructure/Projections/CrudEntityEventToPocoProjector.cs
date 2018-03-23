using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.ReadModel;
using Revo.Domain.Tenancy;

namespace Revo.Infrastructure.Projections
{
    /// <summary>
    /// Common stub for EF6 single-read-model entity projections.
    /// </summary>
    public class CrudEntityEventToPocoProjector<TSource, TTarget> :
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

        protected override async Task<TTarget> CreateProjectionTargetAsync(TSource aggregate, IEnumerable<IEventMessage<DomainAggregateEvent>> events)
        {
            var rm = await Repository.FindAsync<TTarget>(aggregate.Id);
            if (rm != null)
            {
                return rm; //in case we previously did a projection, but didn't succeed in dequeuing async events
            }

            rm = new TTarget();

            if (typeof(ClassEntityReadModel).IsAssignableFrom(typeof(TTarget)))
            {
                var classEntityRm = (ClassEntityReadModel)(object)rm;
                classEntityRm.Id = aggregate.Id;
                classEntityRm.ClassId = typeof(TSource).GetClassId();
            }
            else if (typeof(EntityReadModel).IsAssignableFrom(typeof(TTarget)))
            {
                var entityRm = (EntityReadModel)(object)rm;
                entityRm.Id = aggregate.Id;
            }

            if (aggregate is ITenantOwned tenantOwnedAggregate)
            {
                if (typeof(TenantClassEntityReadModel).IsAssignableFrom(typeof(TTarget)))
                {
                    var tenantClassEntityRm = (TenantClassEntityReadModel)(object) rm;
                    tenantClassEntityRm.TenantId = tenantOwnedAggregate.TenantId;
                }
                else if (typeof(TenantEntityReadModel).IsAssignableFrom(typeof(TTarget)))
                {
                    var tenantEntityRm = (TenantEntityReadModel)(object)rm;
                    tenantEntityRm.TenantId = tenantOwnedAggregate.TenantId;
                }
            }

            Repository.Add(rm);
            return rm;
        }

        protected override Task<TTarget> GetProjectionTargetAsync(TSource aggregate)
        {
            return Repository.GetAsync<TTarget>(aggregate.Id);
        }
    }
}
