using Revo.Domain.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.RavenDB.Projections
{
    public interface IRavenEntityEventProjector<T> : IEntityEventProjector
        where T : IAggregateRoot
    {
    }
}
