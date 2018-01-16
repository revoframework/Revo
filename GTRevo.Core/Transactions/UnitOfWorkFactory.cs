using System;
using System.Linq;
using GTRevo.Core.Events;

namespace GTRevo.Core.Transactions
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IUnitOfWorkProvider[] transactionProviders;
        private readonly IUnitOfWorkListener[] unitOfWorkListeners;
        private readonly Func<IPublishEventBuffer> publishEventBufferFunc;

        public UnitOfWorkFactory(IUnitOfWorkProvider[] transactionProviders,
            IUnitOfWorkListener[] unitOfWorkListeners,
            Func<IPublishEventBuffer> publishEventBufferFunc)
        {
            this.transactionProviders = transactionProviders;
            this.unitOfWorkListeners = unitOfWorkListeners;
            this.publishEventBufferFunc = publishEventBufferFunc;
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            ITransaction[] transactions = transactionProviders
                .Select(x => x.CreateTransaction())
                .ToArray();

            var tx = new UnitOfWork(transactions, unitOfWorkListeners, publishEventBufferFunc());
            return tx;
        }
    }
}
