using System;
using System.Collections.Generic;
using System.Linq;
using Revo.Core.Collections;
using Revo.Domain.Entities;

namespace Revo.Domain.Events
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

        public virtual IReadOnlyCollection<DomainAggregateEvent> UncommitedEvents => uncommittedEvents;

        public virtual void Publish<T>(T evt) where T : DomainAggregateEvent
        {
            evt.AggregateId = aggregate.Id;

            ExecuteHandler(evt);
            uncommittedEvents.Add(evt);
        }

        public virtual void ReplayEvents(IEnumerable<DomainAggregateEvent> events)
        {
            foreach (DomainAggregateEvent ev in events)
            {
                ExecuteHandler(ev);
            }
        }

        public virtual void CommitEvents()
        {
            uncommittedEvents.Clear();
        }

        public virtual void Register<T>(Action<DomainAggregateEvent> handler)
            where T : DomainAggregateEvent
        {
            Register(typeof(T), handler);
        }

        public virtual void Register(Type eventType, Action<DomainAggregateEvent> handler)
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
