using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EF6.EventStores
{
    public class EF6EventStore : EventStore
    {
        public EF6EventStore(ICrudRepository crudRepository, IEventSerializer eventSerializer)
            : base(crudRepository, eventSerializer)
        {
        }

        public override string EventSourceName => "EF6.EventStore";
    }
}

