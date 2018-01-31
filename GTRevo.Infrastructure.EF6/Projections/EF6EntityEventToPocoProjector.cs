using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Core.ReadModel;
using GTRevo.Infrastructure.Core.Tenancy;
using GTRevo.Infrastructure.Projections;

namespace GTRevo.Infrastructure.EF6.Projections
{
    /// <summary>
    /// Common stub for EF6 single-read-model entity projections.
    /// </summary>
    public class EF6EntityEventToPocoProjector<TSource, TTarget> :
        EntityEventToPocoProjector<TSource, TTarget>,
        IEF6EntityEventProjector<TSource>
        where TSource : class, IEventSourcedAggregateRoot
        where TTarget : class, new()
    {
        private readonly ICrudRepository repository;

        public EF6EntityEventToPocoProjector(ICrudRepository repository)
        {
            this.repository = repository;
        }

        public override Task CommitChangesAsync()
        {
            return repository.SaveChangesAsync();
        }

        protected override async Task<TTarget> CreateProjectionTargetAsync(TSource aggregate, IEnumerable<IEventMessage<DomainAggregateEvent>> events)
        {
            var rm = await repository.FindAsync<TTarget>(aggregate.Id);
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

            repository.Add(rm);
            return rm;
        }

        protected override Task<TTarget> GetProjectionTargetAsync(TSource aggregate)
        {
            return repository.GetAsync<TTarget>(aggregate.Id);
        }
    }
}
