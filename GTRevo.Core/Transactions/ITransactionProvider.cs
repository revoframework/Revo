namespace GTRevo.Core.Transactions
{
    public interface ITransactionProvider
    {
        ITransaction CreateTransaction();
    }
}
