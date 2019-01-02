using Revo.Domain.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public interface IEFCoreSyncEntityEventProjector<T> : IEntityEventProjector
        where T : IAggregateRoot
    {
    }
}
