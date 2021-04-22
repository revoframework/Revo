using System;
using System.Collections.Generic;
using Revo.Core.Events;
using Revo.Core.Types;

namespace Revo.Infrastructure.Events.Async.Generic
{
    public class ExternalEventMessageAdapter
    {
        public static IEventMessage FromRecord(ExternalEventRecord record, IEventSerializer eventSerializer)
        {
            var @event = eventSerializer.DeserializeEvent(record.EventJson,
                new VersionedTypeId(record.EventName, record.EventVersion));

            Type messageType = typeof(ExternalEventMessageAdapter<>).MakeGenericType(@event.GetType());
            return (IEventMessage)messageType.GetConstructor(new[] { typeof(ExternalEventRecord), @event.GetType(), typeof(IEventSerializer) })
                .Invoke(new object[] { record, @event, eventSerializer });
        }
    }

    public class ExternalEventMessageAdapter<TEvent> : IEventMessage<TEvent>
        where TEvent : IEvent
    {
        private readonly IEventSerializer eventSerializer;
        private IReadOnlyDictionary<string, string> metadata;

        public ExternalEventMessageAdapter(ExternalEventRecord record, TEvent @event, IEventSerializer eventSerializer)
        {
            Record = record;
            Event = @event;
            this.eventSerializer = eventSerializer;
        }

        public ExternalEventRecord Record { get; }
        public TEvent Event { get; }

        public IReadOnlyDictionary<string, string> Metadata =>
            metadata ??= new LayeredMetadata(eventSerializer.DeserializeEventMetadata(Record.MetadataJson),
                new Dictionary<string, Func<string>>()
                {
                    { BasicEventMetadataNames.EventId, () => Record.Id.ToString() }
                });

        IEvent IEventMessage.Event => Event;
    }
}