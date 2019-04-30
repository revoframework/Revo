using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.EFCore.Repositories;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectionSubSystem : ProjectionSubSystem, IEFCoreProjectionSubSystem, ITransactionParticipant
    {
        private readonly IEFCoreProjectorResolver projectorResolver;
        private readonly Lazy<IEFCoreTransactionCoordinator> transactionCoordinator;

        private readonly HashSet<IEntityEventProjector> allUsedProjectors = new HashSet<IEntityEventProjector>();
        private bool transactionParticipantRegistered = false;
        private EventProjectionOptions eventProjectionOptions;

        public EFCoreProjectionSubSystem(IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory,
            IEFCoreProjectorResolver projectorResolver, Lazy<IEFCoreTransactionCoordinator> transactionCoordinator) : base(entityTypeManager, eventMessageFactory)
        {
            this.projectorResolver = projectorResolver;
            this.transactionCoordinator = transactionCoordinator;
        }

        public override async Task ExecuteProjectionsAsync(
            IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events,
            IUnitOfWork unitOfWork, EventProjectionOptions options)
        {
            if (!transactionParticipantRegistered)
            {
                transactionParticipantRegistered = true;
                transactionCoordinator.Value.AddTransactionParticipant(this);
            }

            await base.ExecuteProjectionsAsync(events, unitOfWork, options);
        }

        protected override IEnumerable<IEntityEventProjector> GetProjectors(Type entityType, EventProjectionOptions options)
        {
            var efCoreOptions = options as EFCoreEventProjectionOptions;

            if (efCoreOptions == null || efCoreOptions.IsSynchronousProjection == false)
            {
                return projectorResolver.GetProjectors(entityType);
            }
            else
            {
                return projectorResolver.GetSyncProjectors(entityType);
            }
        }

        protected override async Task CommitUsedProjectorsAsync(IReadOnlyCollection<IEntityEventProjector> usedProjectors, EventProjectionOptions options)
        {
            eventProjectionOptions = options;

            foreach (var projector in usedProjectors)
            {
                allUsedProjectors.Add(projector);
            }

            await transactionCoordinator.Value.CommitAsync();
        }

        public async Task OnBeforeCommitAsync()
        {
            await base.CommitUsedProjectorsAsync(allUsedProjectors, eventProjectionOptions);
        }

        public Task OnCommitSucceededAsync()
        {
            allUsedProjectors.Clear();
            eventProjectionOptions = null;
            
            return Task.CompletedTask;
        }

        public Task OnCommitFailedAsync()
        {
            allUsedProjectors.Clear();
            eventProjectionOptions = null;

            return Task.CompletedTask;
        }
    }
}
