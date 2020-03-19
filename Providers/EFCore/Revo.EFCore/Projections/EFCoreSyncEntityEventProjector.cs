using Revo.Domain.Entities;
using Revo.EFCore.DataAccess.Entities;

namespace Revo.EFCore.Projections
{
    /// <summary>
    /// A synchronous EF Core CRUD repository-backed event projector for an aggregate type with arbitrary read-model(s).
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    public class EFCoreSyncEntityEventProjector<TSource> : EFCoreEntityEventProjector, IEFCoreSyncEntityEventProjector<TSource>
        where TSource : class, IAggregateRoot
    {
        public EFCoreSyncEntityEventProjector(IEFCoreCrudRepository repository) : base(repository)
        {
        }
    }
}
