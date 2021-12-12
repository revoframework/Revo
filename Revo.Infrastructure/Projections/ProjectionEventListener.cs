using Revo.Core.Commands;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Tenancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Revo.Infrastructure.Projections
{
    public abstract class ProjectionEventListener :
        IAsyncEventListener<DomainAggregateEvent>
    {
        private readonly Func<IProjectionSubSystem> projectionSubSystemFunc;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly Func<CommandContextStack> commandContextStackFunc;
        private readonly ITenantProvider tenantProvider;
        private readonly List<IEventMessage<DomainAggregateEvent>> events = new List<IEventMessage<DomainAggregateEvent>>();

        public ProjectionEventListener(Func<IProjectionSubSystem> projectionSubSystemFunc,
            IUnitOfWorkFactory unitOfWorkFactory, Func<CommandContextStack> commandContextStackFunc,
            ITenantProvider tenantProvider)
        {
            this.projectionSubSystemFunc = projectionSubSystemFunc;
            this.unitOfWorkFactory = unitOfWorkFactory;
            this.commandContextStackFunc = commandContextStackFunc;
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
                using (TenantContextOverride.Push(tenant))
                using (TaskContext.Enter())
                using (IUnitOfWork uow = unitOfWorkFactory.CreateUnitOfWork())
                {
                    var commandContextStack = commandContextStackFunc();
                    commandContextStack.Push(new CommandContext(null, uow));

                    try
                    {
                        uow.Begin();

                        var projectionSubSystem = projectionSubSystemFunc();
                        await projectionSubSystem.ExecuteProjectionsAsync(tenantEvents.ToArray(), uow, GetEventProjectionOptions());

                        await uow.CommitAsync();
                    }
                    finally
                    {
                        commandContextStack.Pop();
                    }
                }
            }

            events.Clear();
        }

        protected virtual EventProjectionOptions GetEventProjectionOptions()
        {
            return new EventProjectionOptions();
        }
    }
}
