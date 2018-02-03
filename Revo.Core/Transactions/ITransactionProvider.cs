namespace Revo.Core.Transactions
{
    public interface ITransactionProvider
    {
        ITransaction CreateTransaction();
    }
}
