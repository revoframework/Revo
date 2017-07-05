using System.Threading.Tasks;
using GTRevo.Core.Transactions;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedRepositoryTransaction : ITransaction
    {
        private readonly EventSourcedRepository eventSourcedRepository;

        public EventSourcedRepositoryTransaction(EventSourcedRepository eventSourcedRepository)
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
