using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Repositories
{
    public interface IRepositoryFactory
    {
        IRepository CreateRepository(IEventQueue eventQueue = null);
    }
}
