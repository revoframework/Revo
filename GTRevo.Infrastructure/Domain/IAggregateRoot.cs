using System.Collections.Generic;

namespace GTRevo.Infrastructure.Domain
{
    public interface IAggregateRoot : IClassEntityBase
    {
        IEnumerable<DomainAggregateEvent> UncommitedEvents { get; }
        int Version { get; }

        void Commit();
    }
}