using Revo.Core.Events;

namespace Revo.Core.Transactions
{
    public interface IUnitOfWork : ITransaction
    {
        IPublishEventBuffer EventBuffer { get; }
    }
}
