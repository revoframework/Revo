using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.Projections;

namespace Revo.Infrastructure.EF6.Projections
{
    public interface IEF6EntityEventProjector<T> : IEntityEventProjector<T>
        where T : IEventSourcedAggregateRoot
    {
    }
}
