using System.Linq;
using GTRevo.Core.Events;

namespace GTRevo.Core.Transactions
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IUnitOfWorkProvider[] transactionProviders;
        private readonly IUnitOfWorkListener[] unitOfWorkListeners;
        private readonly IEventBus eventBus;

        public UnitOfWorkFactory(IUnitOfWorkProvider[] transactionProviders,
            IUnitOfWorkListener[] unitOfWorkListeners, IEventBus eventBus)
        {
            this.transactionProviders = transactionProviders;
            this.unitOfWorkListeners = unitOfWorkListeners;
            this.eventBus = eventBus;
        }

        public ITransaction CreateTransaction()
        {
            ITransaction[] transactions = transactionProviders
                .Select(x => x.CreateTransaction())
                .ToArray();

            var tx = new UnitOfWork(transactions, unitOfWorkListeners, eventBus);
            return tx;
        }
    }
}
