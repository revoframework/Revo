using System.Collections.Generic;
using Revo.Domain.Events;

namespace Revo.Domain.Entities
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