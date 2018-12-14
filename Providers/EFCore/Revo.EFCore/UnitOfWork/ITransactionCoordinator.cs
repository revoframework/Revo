using Revo.Core.Transactions;

namespace Revo.EFCore.UnitOfWork
{
    public interface ITransactionCoordinator
    {
        void AddTransactionParticipant(ITransactionParticipant participant);
    }
}
