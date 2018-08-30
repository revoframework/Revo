using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.Projections;

namespace Revo.RavenDB.Projections
{
    public interface IRavenEntityEventProjector<T> : IEntityEventProjector<T>
        where T : IEventSourcedAggregateRoot
    {
    }
}
