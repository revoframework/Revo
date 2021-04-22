using System;
using System.Threading.Tasks;
using Revo.Core.Events;
using Revo.Infrastructure.Events.Async.Generic;

namespace Revo.Infrastructure.EventStores
{
    /// <summary>
    /// Stores events that don't belong to an event stream, but need to be processed asynchronous listeners anyway.
    /// This mostly includes events from non-event sourced (basic) aggregates and events coming for external source,
    /// e.g. from a message broker.
    /// </summary>
    public interface IExternalEventStore
    {
        Task<ExternalEventRecord> GetEventAsync(Guid eventId);
        Task<ExternalEventRecord[]> CommitAsync();

        /// <summary>
        /// Pushes the event to the store if there is no event with the same ID in the store already.
        /// </summary>
        void TryPushEvent(IEventMessage eventMessage);
    }
}
