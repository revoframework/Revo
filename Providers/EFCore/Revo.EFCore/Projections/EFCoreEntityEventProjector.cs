using System.Threading.Tasks;
using Revo.Domain.Entities;
using Revo.EFCore.DataAccess.Entities;
using Revo.Infrastructure.Projections;

namespace Revo.EFCore.Projections
{
    /// <summary>
    /// An EF6 CRUD repository-backed event projector for an aggregate type with arbitrary read-model(s)..
    /// A convention-based abstract base class that calls an Apply for every event type
    /// and also supports sub-projectors.
    /// </summary>
    /// <typeparam name="TSource">Aggregate type.</typeparam>
    public class EFCoreEntityEventProjector<TSource> :
        EntityEventProjector<TSource>,
        IEFCoreEntityEventProjector<TSource>
        where TSource : class, IAggregateRoot
    {
        public EFCoreEntityEventProjector(IEFCoreCrudRepository repository)
        {
            Repository = repository;
        }

        protected IEFCoreCrudRepository Repository { get; }

        public override async Task CommitChangesAsync()
        {
            await Repository.SaveChangesAsync();
        }
    }
}
