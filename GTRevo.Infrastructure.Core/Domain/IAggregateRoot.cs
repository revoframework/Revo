using System.Collections.Generic;
using GTRevo.Infrastructure.Core.Domain.Events;

namespace GTRevo.Infrastructure.Core.Domain
{
    public interface IAggregateRoot : IEntity
    {
        IReadOnlyCollection<DomainAggregateEvent> UncommittedEvents { get; }
        bool IsChanged { get; }
        bool IsDeleted { get; }
        int Version { get; }

        void Commit();
    }
}