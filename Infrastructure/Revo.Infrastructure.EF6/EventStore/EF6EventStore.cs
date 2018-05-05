using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Types;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Revo.Infrastructure.EF6.Events;
using Revo.Infrastructure.EF6.EventStore.Model;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.EF6.EventStore
{
    public class EF6EventStore : IEventStore
    {
        private const string EventSourceName = "EF6EventStore";

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

        public EF6EventStore(ICrudRepository crudRepository,
            IEventSerializer eventSerializer)
        {
            this.crudRepository = crudRepository;
            this.eventSerializer = eventSerializer;
        }

        public async Task<EventStreamInfo> GetStreamInfoAsync(Guid streamId)
        {
            var lastRow = await QueryStreamRows(streamId, true).FirstOrDefaultAsync();
            if (lastRow == null)
            {
                await GetEventStreamAsync(streamId); //verify stream existence
                return new EventStreamInfo(streamId, 0, 0);
            }
            else
            {
                var eventCount = await QueryStreamRows(streamId).LongCountAsync();
                return new EventStreamInfo(streamId, eventCount, lastRow.StreamSequenceNumber);
            }
        }

        public async Task<IReadOnlyCollection<IEventStoreRecord>> GetEventsAsync(Guid streamId)
        {
            return await ResolveEventRecordsAsync(QueryStreamRows(streamId), streamId);
        }

        public async Task<IReadOnlyCollection<IEventStoreRecord>> GetEventRangeAsync(Guid streamId, long? minSequenceNumber = null,
            long? maxSequenceNumber = null, int? maxCount = null)
        {
            var rows = QueryStreamRows(streamId);
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

            return await ResolveEventRecordsAsync(rows, streamId);
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
                GlobalEF6EventStreamPosition globalPosition = position as GlobalEF6EventStreamPosition;
                if (globalPosition == null)
                {
                    throw new ArgumentException("Invalid IStreamPosition handle passed to EF6EventStore.GetAllEventsBackwardsAsync");
                }

                rows = rows.Where(x => x.GlobalSequenceNumber < globalPosition.LastGlobalSequenceNumberRead);
            }

            if (maxCount != null)
            {
                rows = rows.Take(maxCount.Value);
            }

            var eventRows = await rows.ToListAsync();
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
                newPosition = new GlobalEF6EventStreamPosition(events[0].StreamSequenceNumber);
            }

            EventStreamSlice slice = new EventStreamSlice(events, newPosition);
            return slice;
        }

        public async Task<IReadOnlyDictionary<string, string>> GetStreamMetadataAsync(Guid streamId)
        {
            var eventStream = await GetEventStreamAsync(streamId);
            return eventStream.Metadata;
        }

        public async Task<IEventStoreRecord> GetEventAsync(Guid streamId, long sequenceNumber)
        {
            var row = await QueryStreamRows(streamId)
                .Where(x => x.StreamSequenceNumber == sequenceNumber)
                .FirstOrDefaultAsync();
            if (row == null)
            {
                throw new EntityNotFoundException($"Event with sequence number {sequenceNumber} was not found in stream with ID {streamId}");
            }

            return SelectEventRecordFromRow(row);
        }
        
        public async Task CommitChangesAsync()
        {
            await SetStreamMetadatasAsync();
            await CheckStreamVersionsAsync();
            await crudRepository.SaveChangesAsync();
            ResetChanges();
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
                throw new OptimisticConcurrencyException($"Concurrency exception committing EF6EventStore events: next expected event number for stream ID {streamPair.Key} is {streamVersion + 1}, {minEventNumber} passed");
            }

            if (maxEventNumber - minEventNumber != streamPair.Value.UncommitedRows.Count - 1)
            {
                throw new OptimisticConcurrencyException($"Concurrency exception committing EF6EventStore events: non-sequential event numbers in stream ID {streamPair.Key}");
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

        private IQueryable<EventStreamRow> QueryStreamRows(Guid streamId, bool reverseOrder = false)
        {
            var rows = crudRepository.FindAll<EventStreamRow>()
                .Where(x => x.StreamId == streamId);

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

        private async Task<IReadOnlyCollection<IEventStoreRecord>> ResolveEventRecordsAsync(IQueryable<EventStreamRow> rows, Guid streamId)
        {
            var rowList = await rows.ToListAsync();

            if (rowList.Count == 0
                && await crudRepository.FindAsync<EventStream>(streamId) == null)
            {
                throw new EntityNotFoundException($"No events found for stream ID {streamId}");
            }

            return rowList.Select(SelectEventRecordFromRow).ToList();
        }

        private async Task<EventStream> GetEventStreamAsync(Guid streamId)
        {
            EventStream eventStream = await crudRepository.FindAsync<EventStream>(streamId);
            if (eventStream == null)
            {
                throw new EntityNotFoundException($"Event stream with ID {streamId} not found");
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

            var lastRow = await QueryStreamRows(streamId, true).FirstOrDefaultAsync();
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

