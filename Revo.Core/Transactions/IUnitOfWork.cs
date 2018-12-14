using Revo.Core.Events;

namespace Revo.Core.Transactions
{
    public interface IUnitOfWork : ITransaction
    {
        IPublishEventBuffer EventBuffer { get; }
        bool IsWorkBegun { get; }
        void Begin();
        void AddInnerTransaction(ITransaction innerTransaction);
    }
}
