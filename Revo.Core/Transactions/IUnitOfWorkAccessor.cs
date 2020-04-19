namespace Revo.Core.Transactions
{
    public interface IUnitOfWorkAccessor
    {
        IUnitOfWork UnitOfWork { get; }
    }
}