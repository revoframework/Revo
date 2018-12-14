using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Events;
using Revo.EFCore.UnitOfWork;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public class EFCoreProjectionSubSystem : ProjectionSubSystem, IEFCoreProjectionSubSystem, ITransactionParticipant
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;
        private readonly HashSet<IEntityEventProjector> allUsedProjectors = new HashSet<IEntityEventProjector>();
        private EventProjectionOptions eventProjectionOptions;

        public EFCoreProjectionSubSystem(IEntityTypeManager entityTypeManager, IEventMessageFactory eventMessageFactory,
            IServiceLocator serviceLocator, IEFCoreTransactionCoordinator transactionCoordinator) : base(entityTypeManager, eventMessageFactory)
        {
            this.serviceLocator = serviceLocator;
            this.transactionCoordinator = transactionCoordinator;

            transactionCoordinator.AddTransactionParticipant(this);
        }

        protected override IEnumerable<IEntityEventProjector> GetProjectors(Type entityType, EventProjectionOptions options)
        {
            var efCoreOptions = options as EFCoreEventProjectionOptions;

            if (efCoreOptions == null || efCoreOptions.IsSynchronousProjection == false)
            {
                return serviceLocator.GetAll(
                        typeof(IEFCoreEntityEventProjector<>).MakeGenericType(entityType))
                    .Cast<IEntityEventProjector>();
            }
            else
            {
                return serviceLocator.GetAll(
                        typeof(IEFCoreSyncEntityEventProjector<>).MakeGenericType(entityType))
                    .Cast<IEntityEventProjector>();
            }
        }

        protected override async Task CommitUsedProjectorsAsync(IReadOnlyCollection<IEntityEventProjector> usedProjectors, EventProjectionOptions options)
        {
            eventProjectionOptions = options;

            foreach (var projector in usedProjectors)
            {
                allUsedProjectors.Add(projector);
            }

            await transactionCoordinator.CommitAsync();
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
