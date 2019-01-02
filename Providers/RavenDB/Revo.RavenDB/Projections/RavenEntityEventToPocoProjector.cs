using Revo.Domain.Entities;
using Revo.Infrastructure.Projections;
using Revo.RavenDB.DataAccess;

namespace Revo.RavenDB.Projections
{
    public class RavenEntityEventToPocoProjector<TSource, TTarget> :
        CrudEntityEventToPocoProjector<TSource, TTarget>,
        IRavenEntityEventProjector<TSource>
        where TSource : class, IAggregateRoot
        where TTarget : class, new()
    {
        public RavenEntityEventToPocoProjector(IRavenCrudRepository repository) : base(repository)
        {
            Repository = repository;
        }

        protected new IRavenCrudRepository Repository { get; }
    }
}
