using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using Revo.Core.Core;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventStores.Generic.Model;

namespace Revo.Infrastructure.EventStores.Generic
{
    public class EventStore : IEventStore
    {
        private static readonly string[] FilteredMetadataKeys =
        {
            BasicEventMetadataNames.EventId,
            BasicEventMetadataNames.StreamSequenceNumber,
            BasicEventMetadataNames.StoreDate,
            BasicEventMetadataNames.PublishDate,
        };

        private readonly ICrudRepository crudRepository;
        private readonly IEventSerializer eventSerializer;
        private readonly Dictionary<Guid, StreamBuffer> streams = new Dictionary<Guid, StreamBuffer>();

        public EventStore(ICrudRepository crudRepository,
            IEventSerializer eventSerializer)
        {
            this.crudRepository = crudRepository;
            this.eventSerializer = eventSerializer;
        }

        public virtual string EventSourceName => "Generic.EventStore";

        public async Task<EventStreamInfo> GetStreamInfoAsync(Guid streamId)
        {
            var lastRow = await QueryStreamRows(true, streamId).FirstOrDefaultAsync(crudRepository);
            if (lastRow == null)
            {
                await GetEventStreamAsync(streamId, true); //verify stream existence
                return new EventStreamInfo(streamId, 0, 0);
            }
            else
            {
                var eventCount = await QueryStreamRows(false, streamId).LongCountAsync(crudRepository);
                return new EventStreamInfo(streamId, eventCount, lastRow.StreamSequenceNumber);
            }
        }

        public async Task<IDictionary<Guid, IReadOnlyCollection<IEventStoreRecord>>> BatchFindEventsAsync(Guid[] streamIds)
        {
            return await ResolveEventRecordsAsync(QueryStreamRows(false, streamIds), streamIds, false);
        }

        public async Task<IReadOnlyCollection<IEventStoreRecord>> GetEventsAsync(Guid streamId)
        {
            return await ResolveEventRecordsAsync(QueryStreamRows(false, streamId), streamId, true);
        }

        public async Task<IReadOnlyCollection<IEventStoreRecord>> FindEventsAsync(Guid streamId)
        {
            return await ResolveEventRecordsAsync(QueryStreamRows(false, streamId), streamId, false);
        }

        public async Task<IReadOnlyCollection<IEventStoreRecord>> GetEventRangeAsync(Guid streamId, long? minSequenceNumber = null,
            long? maxSequenceNumber = null, int? maxCount = null)
        {
            var rows = QueryStreamRows(false, streamId);
            if (minSequenceNumber != null)
            {
                rows = rows.Where(x => x.StreamSequenceNumber >= minSequenceNumber.Value);
            }

            if (maxSequenceNumber != null)
            {
                rows = rows.Where(x => x.StreamSequenceNumber <= maxSequenceNumber.Value);
            }

            if (maxCount != null)
            {
                rows = rows.Take(maxCount.Value);
            }

            return await ResolveEventRecordsAsync(rows, streamId, true);
        }

        public void SetStreamMetadata(Guid streamId, IReadOnlyDictionary<string, string> metadata)
        {
            if (!streams.TryGetValue(streamId, out StreamBuffer streamBuffer))
            {
                streamBuffer = streams[streamId] = new StreamBuffer();
            }

            streamBuffer.UncommittedMetadata = metadata.ToImmutableDictionary();
        }

        public async Task<IReadOnlyCollection<IEventStoreRecord>> PushEventsAsync(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events, long? expectedVersion)
        {
            return PushEventsAtVersion(streamId, events, expectedVersion,
                expectedVersion == null ? (long?) await GetStreamVersionAsync(streamId) : null);
        }

        public void AddStream(Guid streamId)
        {
            if (streams.ContainsKey(streamId))
            {
                throw new ArgumentException($"Duplicate ID when adding new event stream: {streamId}");    
            }

            EventStream stream = new EventStream(streamId);
            streams[streamId] = new StreamBuffer() { EventStream = stream, StreamVersion = 0 };
            crudRepository.Add(stream);
        }

        public async Task<EventStreamSlice> GetAllEventsBackwardsAsync(IStreamPosition position = null, int? maxCount = null)
        {
            var rows = crudRepository.FindAll<EventStreamRow>();
            rows = rows.OrderByDescending(x => x.GlobalSequenceNumber);

            if (position != null)
            {
                GlobalEventStreamPosition globalPosition = position as GlobalEventStreamPosition;
                if (globalPosition == null)
                {
                    throw new ArgumentException("Invalid IStreamPosition handle passed to EventStore.GetAllEventsBackwardsAsync");
                }

                rows = rows.Where(x => x.GlobalSequenceNumber < globalPosition.LastGlobalSequenceNumberRead);
            }

            if (maxCount != null)
            {
                rows = rows.Take(maxCount.Value);
            }

            var eventRows = await rows.ToListAsync(crudRepository);
            var events = Enumerable.Range(1, eventRows.Count)
                .Select(i => eventRows[eventRows.Count - i]) //reverse order - despite reading from the end, we still want events to be in oldest-to-newest order
                .Select(SelectEventRecordFromRow)
                .ToList();

            IStreamPosition newPosition = null;
            if (maxCount == 0)
            {
                newPosition = position;
            }
            else if (events.Count > 0 && events[0].StreamSequenceNumber > 1)
            {
                newPosition = new GlobalEventStreamPosition(events[0].StreamSequenceNumber);
            }

            EventStreamSlice slice = new EventStreamSlice(events, newPosition);
            return slice;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetStreamMetadataAsync(Guid streamId)
        {
            var eventStream = await GetEventStreamAsync(streamId, true);
            return eventStream.Metadata;
        }

        public async Task<IReadOnlyDictionary<string, string>> FindStreamMetadataAsync(Guid streamId)
        {
            var eventStream = await GetEventStreamAsync(streamId, false);
            return eventStream?.Metadata;
        }

        public async Task<IReadOnlyDictionary<Guid, IReadOnlyDictionary<string, string>>> BatchFindStreamMetadataAsync(
            Guid[] streamIds)
        {
            var eventStreams = await GetEventStreamsAsync(streamIds, false);
            return eventStreams.ToDictionary(x => x.Id, x => x.Metadata);
        }

        public async Task<IEventStoreRecord> GetEventAsync(Guid streamId, long sequenceNumber)
        {
            var row = await QueryStreamRows(false, streamId)
                .Where(x => x.StreamSequenceNumber == sequenceNumber)
                .FirstOrDefaultAsync(crudRepository);
            if (row == null)
            {
                throw new EntityNotFoundException($"Event with sequence number {sequenceNumber} was not found in stream with ID {streamId}");
            }

            return SelectEventRecordFromRow(row);
        }
        
        public virtual async Task CommitChangesAsync()
        {
            await DoBeforeCommitAsync();

            try
            {
                await crudRepository.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await DoOnCommitFailedAsync();
                throw;
            }

            await DoOnCommitSucceedAsync();
        }

        protected async Task DoBeforeCommitAsync()
        {
            await SetStreamMetadatasAsync();
            await CheckStreamVersionsAsync();
        }

        protected Task DoOnCommitSucceedAsync()
        {
            ResetChanges();
            return Task.CompletedTask;
        }

        protected Task DoOnCommitFailedAsync()
        {
            return Task.CompletedTask;
        }

        private async Task SetStreamMetadatasAsync()
        {
            foreach (var streamBuffer in streams.Where(x => x.Value.UncommittedMetadata != null))
            {
                if (streamBuffer.Value.EventStream == null)
                {
                    streamBuffer.Value.EventStream = await crudRepository.GetAsync<EventStream>(streamBuffer.Key);
                }

                streamBuffer.Value.EventStream.Metadata = streamBuffer.Value.UncommittedMetadata;
                streamBuffer.Value.UncommittedMetadata = null;
            }
        }

        private void CheckStreamVersion(KeyValuePair<Guid, StreamBuffer> streamPair, long streamVersion)
        {
            long minEventNumber = streamPair.Value.UncommitedRows.Values[0].StreamSequenceNumber;
            long maxEventNumber = streamPair.Value.UncommitedRows.Values[streamPair.Value.UncommitedRows.Count - 1].StreamSequenceNumber;

            if (minEventNumber != streamVersion + 1)
            {
                throw new OptimisticConcurrencyException($"Concurrency exception committing {GetType().Name} events: next expected event number for stream ID {streamPair.Key} is {streamVersion + 1}, {minEventNumber} passed");
            }

            if (maxEventNumber - minEventNumber != streamPair.Value.UncommitedRows.Count - 1)
            {
                throw new OptimisticConcurrencyException($"Concurrency exception committing {GetType().Name} events: non-sequential event numbers in stream ID {streamPair.Key}");
            }
        }

        private async Task CheckStreamVersionsAsync()
        {
            foreach (var streamPair in streams)
            {
                if (streamPair.Value.UncommitedRows.Count == 0)
                {
                    continue;
                }

                long streamVersion = await GetStreamVersionAsync(streamPair.Key);
                CheckStreamVersion(streamPair, streamVersion);
            }
        }

        private IReadOnlyCollection<IEventStoreRecord> PushEventsAtVersion(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events,
            long? expectedVersion, long? currentVersion)
        {
            if (!streams.TryGetValue(streamId, out StreamBuffer streamBuffer))
            {
                streamBuffer = streams[streamId] = new StreamBuffer();
            }

            Debug.Assert(expectedVersion != null || currentVersion != null);
            long version = expectedVersion ?? (currentVersion.Value + streamBuffer.UncommitedRows.Count);
            DateTimeOffset storeDate = Clock.Current.Now;

            var rows = events.Select(x =>
            {
                Guid eventId = x.Metadata.GetEventId() ?? Guid.NewGuid();
                var metadata = CreateRowMetadata(x.Metadata);
                var metadataJson = eventSerializer.SerializeEventMetadata(metadata);

                string eventJson;
                VersionedTypeId typeId;
                (eventJson, typeId) = eventSerializer.SerializeEvent(x.Event);

                return new EventStreamRow(eventId, eventJson, typeId.Name, typeId.Version,
                    streamId, ++version, storeDate, metadataJson);
            }).ToList();

            rows.ForEach(x => streamBuffer.UncommitedRows.Add(x.StreamSequenceNumber, x));
            crudRepository.AddRange(rows);

            return rows.Select(SelectEventRecordFromRow).ToList();
        }

        private IReadOnlyDictionary<string, string> CreateRowMetadata(IReadOnlyDictionary<string, string> metadata)
        {
            Dictionary<string, string> newMetadata = metadata
                .Where(x => !FilteredMetadataKeys.Contains(x.Key))
                .ToDictionary(x => x.Key, x => x.Value);
            newMetadata.ReplaceMetadata(BasicEventMetadataNames.EventSourceName, EventSourceName);

            return newMetadata;
        }

        private void ResetChanges()
        {
            streams.Clear();
        }

        private IQueryable<EventStreamRow> QueryStreamRows(bool reverseOrder, params Guid[] streamIds)
        {
            var rows = crudRepository.Where<EventStreamRow>(x => streamIds.Contains(x.StreamId));

            if (!reverseOrder)
            {
                rows = rows.OrderBy(x => x.StreamSequenceNumber);
            }
            else
            {
                rows = rows.OrderByDescending(x => x.StreamSequenceNumber);
            }

            return rows;
        }

        private async Task<IReadOnlyCollection<IEventStoreRecord>> ResolveEventRecordsAsync(IQueryable<EventStreamRow> rows, Guid streamId, bool throwOnStreamEmpty)
        {
            var rowList = await rows.ToListAsync(crudRepository);

            if (throwOnStreamEmpty)
            {
                if (rowList.Count == 0
                    && await crudRepository.FindAsync<EventStream>(streamId) == null)
                {
                    throw new EntityNotFoundException($"No events found for stream ID {streamId}");
                }
            }

            return rowList.Select(SelectEventRecordFromRow).ToList();
        }

        private async Task<Dictionary<Guid, IReadOnlyCollection<IEventStoreRecord>>> ResolveEventRecordsAsync(IQueryable<EventStreamRow> rows, Guid[] streamIds, bool throwOnStreamEmpty)
        {
            var rowList = await rows.ToListAsync(crudRepository);

            var streamEvents = rowList.GroupBy(x => x.StreamId)
                .ToDictionary(x => x.Key,
                    x => (IReadOnlyCollection<IEventStoreRecord>) x.Select(SelectEventRecordFromRow).ToList());

            if (throwOnStreamEmpty)
            {
                foreach (var streamPair in streamEvents)
                {
                    if (streamPair.Value.Count == 0
                        && await crudRepository.FindAsync<EventStream>(streamPair.Key) == null)
                    {
                        throw new EntityNotFoundException($"No events found for stream ID {streamPair.Key}");
                    }
                }
            }

            return streamEvents;
        }

        private async Task<EventStream> GetEventStreamAsync(Guid streamId, bool throwOnNotFound)
        {
            EventStream eventStream = await crudRepository.FindAsync<EventStream>(streamId);
            if (eventStream == null)
            {
                if (throwOnNotFound)
                {
                    throw new EntityNotFoundException($"Event stream with ID {streamId} not found");
                }
                else
                {
                    return null;
                }
            }

            if (streams.TryGetValue(streamId, out StreamBuffer streamBuffer))
            {
                if (streamBuffer.EventStream == null)
                {
                    streamBuffer.EventStream = eventStream;
                }
            }
            else
            {
                streams[streamId] = new StreamBuffer() { EventStream = eventStream };
            }

            return eventStream;
        }

        private async Task<List<EventStream>> GetEventStreamsAsync(Guid[] streamIds, bool throwOnNotFound)
        {
            var result = new List<EventStream>();

            StreamBuffer streamBuffer;
            List<Guid> missingStreamIds = null;
            foreach (Guid streamId in streamIds)
            {
                if (streams.TryGetValue(streamId, out streamBuffer)
                    && streamBuffer.EventStream != null)
                {
                    result.Add(streamBuffer.EventStream);
                }
                else
                {
                    if (missingStreamIds == null)
                    {
                        missingStreamIds = new List<Guid>();
                    }

                    missingStreamIds.Add(streamId);
                }
            }

            if (missingStreamIds?.Count > 0)
            {
                EventStream[] loadedEventStreams = await crudRepository.Where<EventStream>(x => missingStreamIds.Contains(x.Id)).ToArrayAsync(crudRepository);
                if (throwOnNotFound)
                {
                    var notFoundIds = missingStreamIds.Where(x => !loadedEventStreams.Any(y => y.Id == x)).ToArray();
                    if (notFoundIds.Length > 0)
                    {
                        throw new EntityNotFoundException($"Event stream(s) with ID(s) {string.Join(", ", notFoundIds)} not found");
                    }
                }

                foreach (var eventStream in loadedEventStreams)
                {
                    if (streams.TryGetValue(eventStream.Id, out streamBuffer))
                    {
                        streamBuffer.EventStream = eventStream;
                    }
                    else
                    {
                        streams[eventStream.Id] = new StreamBuffer() { EventStream = eventStream };
                    }
                }

                result.AddRange(loadedEventStreams);
            }
            
            return result;
        }

        private IEventStoreRecord SelectEventRecordFromRow(EventStreamRow row)
        {
            return new EventStoreRecordAdapter(row, eventSerializer);
        }

        private async Task<long> GetStreamVersionAsync(Guid streamId)
        {
            if (streams.TryGetValue(streamId, out StreamBuffer streamBuffer)
                && streamBuffer.StreamVersion != null)
            {
                return streamBuffer.StreamVersion.Value;
            }

            var lastRow = await QueryStreamRows(true, streamId).FirstOrDefaultAsync(crudRepository);
            if (lastRow != null)
            {
                return lastRow.StreamSequenceNumber;
            }
            else
            {
                return 0;
            }
        }
        
        private class StreamBuffer
        {
            public EventStream EventStream { get; set; }
            public ImmutableDictionary<string, string> UncommittedMetadata { get; set; }
            public long? StreamVersion { get; set; }
            public SortedList<long, EventStreamRow> UncommitedRows { get; } = new SortedList<long, EventStreamRow>();
        }
    }
}

