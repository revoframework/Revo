using System.Collections.Generic;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.DataAccess.EF6.Entities;
using Revo.DataAccess.Entities;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Domain.ReadModel;
using Revo.Domain.Tenancy;
using Revo.Infrastructure.Projections;

namespace Revo.Infrastructure.EF6.Projections
{
    /// <summary>
    /// Common stub for EF6 single-read-model entity projections.
    /// </summary>
    public class EF6EntityEventToPocoProjector<TSource, TTarget> :
        CrudEntityEventToPocoProjector<TSource, TTarget>,
        IEF6EntityEventProjector<TSource>
        where TSource : class, IEventSourcedAggregateRoot
        where TTarget : class, new()
    {
        public EF6EntityEventToPocoProjector(IEF6CrudRepository repository)  : base(repository)
        {
            Repository = repository;
        }

        protected new IEF6CrudRepository Repository { get; }
    }
}
