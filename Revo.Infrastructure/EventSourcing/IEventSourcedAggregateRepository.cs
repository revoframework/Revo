using Revo.Domain.Entities.EventSourcing;

namespace Revo.Infrastructure.EventSourcing
{
    public interface IEventSourcedAggregateRepository : IEventSourcedRepository<IEventSourcedAggregateRoot>
    {
    }
}
