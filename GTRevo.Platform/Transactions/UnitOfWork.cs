using System.Linq;
using GTRevo.Platform.Events;

namespace GTRevo.Platform.Transactions
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ITransactionProvider[] transactionProviders;
        private readonly IUnitOfWorkListener[] unitOfWorkListeners;

        public UnitOfWork(ITransactionProvider[] transactionProviders,
            IUnitOfWorkListener[] unitOfWorkListeners)
        {
            this.transactionProviders = transactionProviders;
            this.unitOfWorkListeners = unitOfWorkListeners;
        }

        public ITransaction CreateTransaction()
        {
            ITransaction[] transactions = transactionProviders
                .Select(x => x.CreateTransaction())
                .ToArray();

            var tx = new UnitOfWorkTransaction(transactions, unitOfWorkListeners);

            foreach (var listener in unitOfWorkListeners)
            {
                listener.OnTransactionBeginned(tx);
            }

            return tx;
        }
    }
}
