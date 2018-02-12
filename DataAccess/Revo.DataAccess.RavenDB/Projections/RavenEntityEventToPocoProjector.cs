using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.DataAccess.RavenDB.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Infrastructure.Projections;

namespace Revo.DataAccess.RavenDB.Projections
{
    public class RavenEntityEventToPocoProjector<TSource, TTarget> :
        CrudEntityEventToPocoProjector<TSource, TTarget>,
        IRavenEntityEventProjector<TSource>
        where TSource : class, IEventSourcedAggregateRoot
        where TTarget : class, new()
    {
        public RavenEntityEventToPocoProjector(IRavenCrudRepository repository) : base(repository)
        {
        }
    }
}
