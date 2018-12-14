using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.UnitOfWork
{
    public class EFCoreCrudAggregateStore : CrudAggregateStore, ITransactionParticipant
    {
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreCrudAggregateStore(ICrudRepository crudRepository, IEntityTypeManager entityTypeManager,
            IPublishEventBuffer publishEventBuffer, IEventMessageFactory eventMessageFactory,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(crudRepository, entityTypeManager, publishEventBuffer, eventMessageFactory)
        {
            this.transactionCoordinator = transactionCoordinator;
            transactionCoordinator.AddTransactionParticipant(this);
        }

        public override bool NeedsSave => false;

        public override Task SaveChangesAsync()
        {
            return transactionCoordinator.CommitAsync();
        }

        public Task OnBeforeCommitAsync()
        {
            InjectClassIds();
            return Task.CompletedTask;
        }

        public async Task OnCommitSucceededAsync()
        {
            await CommitAggregatesAsync();
        }

        public Task OnCommitFailedAsync()
        {
            return Task.CompletedTask;
        }
    }
}
