using System.Collections.Generic;
using System.Linq;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.Repositories;

namespace Revo.EFCore.Repositories
{
    public class EFCoreEventSourcedAggregateStoreFactory : EventSourcedAggregateStoreFactory
    {
        private readonly IEventStore eventStore;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IRepositoryFilter[] repositoryFilters;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly IEventSourcedAggregateFactory eventSourcedAggregateFactory;
        private readonly IEFCoreTransactionCoordinator transactionCoordinator;

        public EFCoreEventSourcedAggregateStoreFactory(
            IEventStore eventStore, IEntityTypeManager entityTypeManager,
            IEnumerable<IRepositoryFilter> repositoryFilters, IEventMessageFactory eventMessageFactory,
            IEventSourcedAggregateFactory eventSourcedAggregateFactory,
            IEFCoreTransactionCoordinator transactionCoordinator)
            : base(eventStore, entityTypeManager, repositoryFilters, eventMessageFactory, eventSourcedAggregateFactory)
        {
            this.eventStore = eventStore;
            this.entityTypeManager = entityTypeManager;
            this.repositoryFilters = repositoryFilters.ToArray();
            this.eventMessageFactory = eventMessageFactory;
            this.eventSourcedAggregateFactory = eventSourcedAggregateFactory;
            this.transactionCoordinator = transactionCoordinator;
        }

        public override IAggregateStore CreateAggregateStore(IUnitOfWork unitOfWork)
        {
            return new EFCoreEventSourcedAggregateStore(eventStore, entityTypeManager,
                unitOfWork?.EventBuffer, repositoryFilters, eventMessageFactory,
                eventSourcedAggregateFactory, transactionCoordinator);
        }
    }
}
