namespace GTRevo.Platform.Transactions
{
    public interface ITransactionProvider
    {
        ITransaction CreateTransaction();
    }
}
