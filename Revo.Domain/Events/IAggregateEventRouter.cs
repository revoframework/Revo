using System;
using System.Collections.Generic;

namespace Revo.Domain.Events
{
    public interface IAggregateEventRouter
    {
        IReadOnlyCollection<DomainAggregateEvent> UncommitedEvents { get; }

        void Publish<T>(T evt) where T : DomainAggregateEvent;
        void CommitEvents();
        void Register(Type eventType, Action<DomainAggregateEvent> handler);
        void Register<T>(Action<DomainAggregateEvent> handler) where T : DomainAggregateEvent;
        void ReplayEvents(IEnumerable<DomainAggregateEvent> events);
    }
}