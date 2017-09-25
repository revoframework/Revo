using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;

namespace GTRevo.Infrastructure.EventSourcing
{
    public class EventSourcedAggregateRepository : EventSourcedRepository<IEventSourcedAggregateRoot>, IEventSourcedAggregateRepository
    {
        public EventSourcedAggregateRepository(IEventStore eventStore, IActorContext actorContext,
            IEntityTypeManager entityTypeManager, IEventQueue eventQueue)
            : base(eventStore, actorContext, entityTypeManager, eventQueue)
        {
        }
    }
}
