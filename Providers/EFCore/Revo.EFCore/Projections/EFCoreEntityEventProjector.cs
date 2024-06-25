using Revo.Domain.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    /// <summary>
    /// An EF Core CRUD repository-backed event projector with arbitrary read-model(s).
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    public abstract class EFCoreEntityEventProjector : EntityEventProjector
    {
        public EFCoreEntityEventProjector(IEFCoreCrudRepository repository)
        {
            Repository = repository;
        }

        protected IEFCoreCrudRepository Repository { get; }
    }

    /// <summary>
    /// An EF Core CRUD repository-backed event projector for an aggregate type with arbitrary read-model(s).
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    public abstract class EFCoreEntityEventProjector<TSource> : EFCoreEntityEventProjector, IEFCoreEntityEventProjector<TSource>
        where TSource : class, IAggregateRoot
    {
        public EFCoreEntityEventProjector(IEFCoreCrudRepository repository) : base(repository)
        {
        }
    }
}
