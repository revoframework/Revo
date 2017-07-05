using System.Collections.Generic;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain
{
    public interface IAggregateRoot : IEntity
    {
        IEnumerable<DomainAggregateEvent> UncommitedEvents { get; }
        int Version { get; }

        void Commit();
    }
}