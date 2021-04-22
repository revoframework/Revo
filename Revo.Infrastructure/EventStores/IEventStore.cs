using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Revo.Infrastructure.EventStores
{
    /// <summary>
    /// Stores event streams and their events.
    /// </summary>
    public interface IEventStore
    {
        /// <summary>
        /// Adds new event stream.
        /// </summary>
        /// <param name="streamId"></param>
        void AddStream(Guid streamId);

        Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, string>>> BatchFindStreamMetadataAsync(Guid[] streamIds);
        Task<IDictionary<Guid, IReadOnlyCollection<IEventStoreRecord>>> BatchFindEventsAsync(Guid[] streamIds);

        Task<IReadOnlyCollection<IEventStoreRecord>> FindEventsAsync(Guid streamId);
        Task<IReadOnlyDictionary<string, string>> FindStreamMetadataAsync(Guid streamId);

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

        void SetStreamMetadata(Guid streamId, IReadOnlyDictionary<string, string> metadata);

        Task<IReadOnlyCollection<IEventStoreRecord>> PushEventsAsync(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events);
        
        Task CommitChangesAsync();
    }
}
