using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.EF6.EventStore.Model;
using GTRevo.Infrastructure.EventStore;

namespace GTRevo.Infrastructure.EF6.EventStore
{
    public class EF6EventStore : IEventStore
    {
        private readonly ICrudRepository crudRepository;
        private readonly IDomainEventTypeCache domainEventTypeCache;
        private readonly Dictionary<Guid, StreamBuffer> streams = new Dictionary<Guid, StreamBuffer>();

        public EF6EventStore(ICrudRepository crudRepository,
            IDomainEventTypeCache domainEventTypeCache)
        {
            this.crudRepository = crudRepository;
            this.domainEventTypeCache = domainEventTypeCache;
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

        public async Task SetStreamMetadataAsync(Guid streamId, IReadOnlyDictionary<string, string> metadata)
        {
            var stream = await crudRepository.GetAsync<EventStream>(streamId);
            stream.Metadata = metadata;
        }

        public void PushEvents(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events, long? expectedVersion)
        {
            throw new NotImplementedException();
        }

        public async Task PushEventsAsync(Guid streamId, IEnumerable<IUncommittedEventStoreRecord> events, long? expectedVersion)
        {
            if (!streams.TryGetValue(streamId, out StreamBuffer streamBuffer))
            {
                streamBuffer = streams[streamId] = new StreamBuffer();
            }

            long version = expectedVersion ?? (await GetStreamVersionAsync(streamId) + streamBuffer.UncommitedRows.Count);
            DateTimeOffset storeDate = Clock.Current.Now;

            var rows = events.Select(x =>
            {
                Guid eventId = x.Metadata.GetEventId() ?? Guid.NewGuid();
                return new EventStreamRow(domainEventTypeCache, eventId, x.Event, streamId,
                    ++version, storeDate, x.Metadata);
            }).ToList();

            rows.ForEach(x => streamBuffer.UncommitedRows.Add(x.StreamSequenceNumber, x));
            crudRepository.AddRange(rows);
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

        public void CommitChanges()
        {
            throw new NotImplementedException();
        }

        public async Task CommitChangesAsync()
        {
            await CheckStreamVersionsAsync();
            await crudRepository.SaveChangesAsync();
            ResetChanges();
        }

        private async Task CheckStreamVersionsAsync()
        {
            //checking gaps
            foreach (var streamPair in streams)
            {
                if (streamPair.Value.UncommitedRows.Count == 0)
                {
                    continue;
                }

                long streamVersion = await GetStreamVersionAsync(streamPair.Key);
                long minEventNumber = streamPair.Value.UncommitedRows[0].StreamSequenceNumber;
                long maxEventNumber = streamPair.Value.UncommitedRows[streamPair.Value.UncommitedRows.Count - 1].StreamSequenceNumber;

                if (minEventNumber != streamVersion + 1)
                {
                    throw new OptimisticConcurrencyException($"Concurrency exception committing EF6EventStore events: next expected event number for stream ID {streamPair.Key} is {streamVersion + 1}, {minEventNumber} passed");
                }

                if (maxEventNumber - minEventNumber != streamPair.Value.UncommitedRows.Count - 1)
                {
                    throw new OptimisticConcurrencyException($"Concurrency exception committing EF6EventStore events: non-sequential event numbers in stream ID {streamPair.Key}");
                }
            }
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
            row.DomainEventTypeCache = domainEventTypeCache;
            return row;
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
            public long? StreamVersion { get; set; }
            public SortedList<long, EventStreamRow> UncommitedRows { get; } = new SortedList<long, EventStreamRow>();
        }
    }
}
