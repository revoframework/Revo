using Revo.Domain.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EF6.Projections
{
    /// <summary>
    /// An EF6 CRUD repository-backed event projector for an aggregate type with a single POCO read model for every aggregate.
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    /// <remarks>
    /// Automatically creates, loads and saves read model instances using the repository.
    /// If TTarget is IManuallyRowVersioned, automatically handles read model versioning and projection
    /// idempotency using event sequence numbers.
    /// Furthermore, automatically sets read model IDs for IEntityReadModel,
    /// class IDs for IClassEntityReadModel and tenant IDs for ITenantReadModel.
    /// </remarks>
    /// <typeparam name="TSource">Aggregate type.</typeparam>
    /// <typeparam name="TTarget">Read model type. Should have a parameterless constructor.</typeparam>
    public class EF6EntityEventToPocoProjector<TSource, TTarget> :
        CrudEntityEventToPocoProjector<TSource, TTarget>,
        IEF6EntityEventProjector<TSource>
        where TSource : class, IAggregateRoot
        where TTarget : class, new()
    {
        public EF6EntityEventToPocoProjector(IEF6CrudRepository repository)  : base(repository)
        {
            Repository = repository;
        }

        protected new IEF6CrudRepository Repository { get; }
    }
}
