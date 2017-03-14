using System;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.Domain.Projections
{
    public interface ICrudEntityProjector
    {
        Type ProjectedAggregateType { get; }
    }

    public interface ICrudEntityProjector<in T> : ICrudEntityProjector
        where T : IAggregateRoot
    {
        Task ProjectChangesAsync(T aggregate);
    }
}
