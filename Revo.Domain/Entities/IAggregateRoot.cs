using System.Collections.Generic;
using Revo.Domain.Events;

namespace Revo.Domain.Entities
{
    /// <summary>
    /// Root entity of an aggregate.
    /// </summary>
    public interface IAggregateRoot : IEntity
    {
        IReadOnlyCollection<DomainAggregateEvent> UncommittedEvents { get; }
        bool IsChanged { get; }
        bool IsDeleted { get; }
        int Version { get; }

        /// <summary>
        /// Commits the changes (e.g. events) internally buffered by the aggregate and increases the version.
        /// You don't need to call this explicitly as it is done automatically by the repository after
        /// saving the aggregate.
        /// </summary>
        void Commit();
    }
}