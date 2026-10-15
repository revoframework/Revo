using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Tenancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Revo.Infrastructure.Projections
{
    public abstract class ProjectionEventListener :
        IAsyncEventListener<DomainAggregateEvent>
    {
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ITenantProvider tenantProvider;
        private readonly List<IEventMessage<DomainAggregateEvent>> events = new List<IEventMessage<DomainAggregateEvent>>();

        public ProjectionEventListener(IServiceScopeFactory serviceScopeFactory, ITenantProvider tenantProvider)
        {
            this.serviceScopeFactory = serviceScopeFactory;
            this.tenantProvider = tenantProvider;
        }

        public abstract IAsyncEventSequencer EventSequencer { get; }

        public Task HandleAsync(IEventMessage<DomainAggregateEvent> message, string sequenceName)
        {
            events.Add(message);
            return Task.FromResult(0);
        }

        public Task OnFinishedEventQueueAsync(string sequenceName)
        {
            return ExecuteProjectionsAsync();
        }

        public async Task ExecuteProjectionsAsync()
        {
            var eventsByTenant = events.GroupBy(x => x.Metadata.GetAggregateTenantId());
            foreach (var tenantEvents in eventsByTenant)
            {
                var tenant = tenantProvider.GetTenant(tenantEvents.Key);

                // tenant override needs to be pushed before the scope is created, so that all of its services
                // resolve the tenant context of the projected events
                using (TenantContextOverride.Push(tenant))
                {
                    await serviceScopeFactory.ExecuteInUnitOfWorkScopeAsync(async (serviceProvider, uow) =>
                    {
                        var projectionSubSystem = GetProjectionSubSystem(serviceProvider);
                        await projectionSubSystem.ExecuteProjectionsAsync(tenantEvents.ToArray(), uow,
                            GetEventProjectionOptions());
                    });
                }
            }

            events.Clear();
        }

        /// <summary>
        /// Resolves the projection sub-system to use from the projection's service scope.
        /// </summary>
        protected abstract IProjectionSubSystem GetProjectionSubSystem(IServiceProvider serviceProvider);

        protected virtual EventProjectionOptions GetEventProjectionOptions()
        {
            return new EventProjectionOptions();
        }
    }
}
