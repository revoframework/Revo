using Revo.Domain.Entities.EventSourcing;

namespace Revo.Infrastructure.EventSourcing
{
    internal interface IEventSourcedAggregateRepository : IEventSourcedRepository<IEventSourcedAggregateRoot>
    {
    }
}
