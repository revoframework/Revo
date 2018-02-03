using System.Threading.Tasks;
using Revo.Core.Transactions;
using Revo.Domain.Entities.EventSourcing;

namespace Revo.Infrastructure.EventSourcing
{
    public class EventSourcedRepositoryTransaction<T> : ITransaction
        where T : class, IEventSourcedAggregateRoot
    {
    private readonly EventSourcedRepository<T> eventSourcedRepository;

    public EventSourcedRepositoryTransaction(EventSourcedRepository<T> eventSourcedRepository)
    {
        this.eventSourcedRepository = eventSourcedRepository;
    }

    public void Commit()
    {
        eventSourcedRepository.SaveChanges();
    }

    public Task CommitAsync()
    {
        return eventSourcedRepository.SaveChangesAsync();
    }

    public void Dispose()
    {
    }
    }
}
