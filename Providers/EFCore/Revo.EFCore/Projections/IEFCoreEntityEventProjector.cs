using Revo.Domain.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    public interface IEFCoreEntityEventProjector<T> : IEntityEventProjector<T>
        where T : IAggregateRoot
    {
    }
}
