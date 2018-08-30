using Revo.DataAccess.Entities;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores.Generic;

namespace Revo.EFCore.EventStores
{
    public class EFCoreEventStore : EventStore
    {
        public EFCoreEventStore(ICrudRepository crudRepository, IEventSerializer eventSerializer)
            : base(crudRepository, eventSerializer)
        {
        }

        public override string EventSourceName => "EFCore.EventStore";
    }
}
