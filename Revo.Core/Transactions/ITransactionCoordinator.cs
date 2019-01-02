namespace Revo.Core.Transactions
{
    public interface ITransactionCoordinator
    {
        void AddTransactionParticipant(ITransactionParticipant participant);
    }
}
