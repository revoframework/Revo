using Revo.Domain.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EF6.Projections
{
    public interface IEF6EntityEventProjector<T> : IEntityEventProjector
        where T : IAggregateRoot
    {
    }
}
