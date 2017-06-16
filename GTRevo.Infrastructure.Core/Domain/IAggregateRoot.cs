using System.Collections.Generic;
using GTRevo.Infrastructure.Domain.Events;

namespace GTRevo.Infrastructure.Domain
{
    public interface IAggregateRoot : IEntity
    {
        IEnumerable<DomainAggregateEvent> UncommitedEvents { get; }
        int Version { get; }

        void Commit();
    }
}