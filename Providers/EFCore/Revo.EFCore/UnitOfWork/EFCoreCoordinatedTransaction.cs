using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Commands;
using Revo.Core.Transactions;
using Revo.EFCore.DataAccess.Entities;
using Revo.EFCore.Events;
using Revo.EFCore.EventStores;
using Revo.EFCore.Projections;
using Revo.Infrastructure.Events;

namespace Revo.EFCore.UnitOfWork
{
    public class EFCoreCoordinatedTransaction : CoordinatedTransaction, IEFCoreTransactionCoordinator, IUnitOfWorkListener
    {
        private readonly IEFCoreCrudRepository crudRepository;
        private readonly Lazy<EFCoreExternalEventStoreHook> efCoreExternalEventStoreHookLazy;
        private readonly Lazy<EFCoreSyncProjectionHook> efCoreSyncProjectionHook;

        public EFCoreCoordinatedTransaction(IEFCoreCrudRepository crudRepository,
            ICommandContext commandContext,
            Lazy<EFCoreExternalEventStoreHook> efCoreExternalEventStoreHookLazy,
            Lazy<EFCoreSyncProjectionHook> efCoreSyncProjectionHook) : base(new EFCoreCrudRepositoryTransaction(crudRepository))
        {
            this.crudRepository = crudRepository;
            this.efCoreExternalEventStoreHookLazy = efCoreExternalEventStoreHookLazy;
            this.efCoreSyncProjectionHook = efCoreSyncProjectionHook;

            if (commandContext.UnitOfWork?.IsWorkBegun == true)
            {
                commandContext.UnitOfWork.AddInnerTransaction(this);
            }
        }

        protected override async Task DoCommitAsync()
        {
            if (!Participants.Any(x => x is EFCoreExternalEventStoreHook))
            {
                Participants.Add(efCoreExternalEventStoreHookLazy.Value);
            }

            if (!Participants.Any(x => x is EFCoreSyncProjectionHook))
            {
                Participants.Add(efCoreSyncProjectionHook.Value);
            }

            Participants = Participants.OrderBy(x =>
            {
                switch (x)
                {
                    case EFCoreEventSourcedAggregateStore _:
                        return 100;

                    case EFCoreCrudAggregateStore _:
                        return 101;

                    case EFCoreSyncProjectionHook _:
                        return 102;

                    case EFCoreProjectionSubSystem _:
                        return 103;
                        
                    case EFCoreExternalEventStoreHook _:
                        return 104;

                    case EFCoreEventStore _:
                        return 105;

                    case EFCoreExternalEventStore _:
                        return 106;

                    case EFCoreAsyncEventQueueManager _:
                        return 107;

                    default:
                        return 0;
                }
            }).ToList();

            await base.DoCommitAsync();
        }

        public Task OnBeforeWorkCommitAsync(IUnitOfWork unitOfWork)
        {
            return Task.CompletedTask;
        }

        public void OnWorkBegin(IUnitOfWork unitOfWork)
        {
            unitOfWork.AddInnerTransaction(this);
        }

        public Task OnWorkSucceededAsync(IUnitOfWork unitOfWork)
        {
            crudRepository.DatabaseAccess.ClearDbContexts();
            return Task.CompletedTask;
        }

        public class EFCoreCrudRepositoryTransaction : ITransaction
        {
            private readonly IEFCoreCrudRepository crudRepository;

            public EFCoreCrudRepositoryTransaction(IEFCoreCrudRepository crudRepository)
            {
                this.crudRepository = crudRepository;
            }

            public void Dispose()
            {
            }

            public Task CommitAsync()
            {
                return crudRepository.SaveChangesAsync();
            }
        }
    }
}
