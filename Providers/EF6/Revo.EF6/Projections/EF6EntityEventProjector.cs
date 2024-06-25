using Revo.Domain.Entities;
using Revo.EF6.DataAccess.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EF6.Projections
{
    /// <summary>
    /// An EF6 CRUD repository-backed event projector for an aggregate type with arbitrary read-model(s)..
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    /// <typeparam name="TSource">Aggregate type.</typeparam>
    public class EF6EntityEventProjector<TSource>(IEF6CrudRepository repository) :
        EntityEventProjector,
        IEF6EntityEventProjector<TSource>
        where TSource : class, IAggregateRoot
    {
        protected IEF6CrudRepository Repository { get; } = repository;
    }
}
