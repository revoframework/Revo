using Revo.Core.Events;

namespace Revo.Infrastructure.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository CreateRepository(IPublishEventBuffer eventQueue = null);
    }
}
