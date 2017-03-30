using System;
using System.Collections.Generic;
using System.Linq;

namespace GTRevo.Infrastructure.Domain
{
    public class AggregateEventRouter : IAggregateEventRouter
    {
        private readonly IAggregateRoot aggregate;

        private readonly MultiValueDictionary<Type, Action<DomainAggregateEvent>> handlers =
            new MultiValueDictionary<Type, Action<DomainAggregateEvent>>();

        private readonly List<DomainAggregateEvent> uncommittedEvents = new List<DomainAggregateEvent>();

        public AggregateEventRouter(IAggregateRoot aggregate)
        {
            this.aggregate = aggregate;
        }

        public virtual IEnumerable<DomainAggregateEvent> UncommitedEvents => uncommittedEvents;

        public void ApplyEvent<T>(T evt) where T : DomainAggregateEvent
        {
            evt.AggregateId = aggregate.Id;
            evt.AggregateClassId = aggregate.ClassId;

            ExecuteHandler(evt);
            uncommittedEvents.Add(evt);
        }

        public void ReplayEvents(IEnumerable<DomainAggregateEvent> events)
        {
            foreach (DomainAggregateEvent ev in events)
            {
                ExecuteHandler(ev);
            }
        }

        public void CommitEvents()
        {
            uncommittedEvents.Clear();
        }

        public void Register<T>(Action<DomainAggregateEvent> handler)
            where T : DomainAggregateEvent
        {
            Register(typeof(T), handler);
        }

        public void Register(Type eventType, Action<DomainAggregateEvent> handler)
        {
            if (!typeof(DomainAggregateEvent).IsAssignableFrom(eventType))
            {
                throw new ArgumentException("Invalid domain aggregate event type for router registration: " +
                                            eventType.FullName);
            }

            handlers.Add(eventType, handler);
        }

        private void ExecuteHandler<T>(T evt) where T : DomainAggregateEvent
        {
            IReadOnlyCollection<Action<DomainAggregateEvent>> eventHandlers;
            if (handlers.TryGetValue(evt.GetType(), out eventHandlers))
            {
                foreach (var handler in eventHandlers.ToList()) //handler may create new sub-entities that create new registrations in the meantime
                {
                    handler.Invoke(evt);
                }
            }
        }
    }
}
