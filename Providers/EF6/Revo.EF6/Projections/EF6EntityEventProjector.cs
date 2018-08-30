using System.Threading.Tasks;
using Revo.Domain.Entities.EventSourcing;
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
    public class EF6EntityEventProjector<TSource> :
        EntityEventProjector<TSource>,
        IEF6EntityEventProjector<TSource>
        where TSource : class, IEventSourcedAggregateRoot
    {
        public EF6EntityEventProjector(IEF6CrudRepository repository)
        {
            Repository = repository;
        }

        protected IEF6CrudRepository Repository { get; }

        public override async Task CommitChangesAsync()
        {
            await Repository.SaveChangesAsync();
        }
    }
}
