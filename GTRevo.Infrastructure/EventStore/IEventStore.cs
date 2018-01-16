using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GTRevo.Infrastructure.EventStore
{
    public interface IEventStore
    {
        /// <summary>
        /// Adds new event stream.
        /// </summary>
        /// <param name="streamId"></param>
        void AddStream(Guid streamId);

        /// <summary>
        /// Reads events from all streams at once in backward-order (newest first).
        /// </summary>
        /// <param name="position">Position returned from previous call to GetAllEventsBackwardsAsync or null.</param>
        /// <param name="maxCount"></param>
        /// <returns></returns>
        Task<EventStreamSlice> GetAllEventsBackwardsAsync(IStreamPosition position = null,
            int? maxCount = null);

        Task<IEventStoreRecord> GetEventAsync(Guid streamId, long sequenceNumber);
        Task<IReadOnlyCollection<IEventStoreRecord>> GetEventsAsync(Guid streamId);
        Task<IReadOnlyCollection<IEventStoreRecord>> GetEventRangeAsync(Guid streamId, long? minSequenceNumber = null,
            long? maxSequenceNumber = null, int? maxCount = null);
        Task<IReadOnlyDictionary<string, string>> GetStreamMetadataAsync(Guid streamId);
        Task<EventStreamInfo> GetStreamInfoAsync(Guid streamId);

        // TODO: reading without metadata

        Task SetStreamMetadataAsync(Guid streamId, IReadOnlyDictionary<string, string> metadata);

        IReadOnlyCollection<IEventStoreRecord> PushEvents(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events, long? expectedVersion);
        Task<IReadOnlyCollection<IEventStoreRecord>> PushEventsAsync(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events, long? expectedVersion);

        void CommitChanges();
        Task CommitChangesAsync();
    }
}
