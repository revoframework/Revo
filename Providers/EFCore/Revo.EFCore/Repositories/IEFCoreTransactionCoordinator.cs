using Revo.Core.Transactions;

namespace Revo.EFCore.Repositories
{
    public interface IEFCoreTransactionCoordinator : ITransactionCoordinator, ITransaction
    {
    }
}
