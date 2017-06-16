namespace GTRevo.Transactions
{
    public interface ITransactionProvider
    {
        ITransaction CreateTransaction();
    }
}
