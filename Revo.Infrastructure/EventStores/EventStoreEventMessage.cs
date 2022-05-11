using System;
using System.Collections.Generic;
using System.Globalization;
using Revo.Core.Core;
using Revo.Core.Events;

namespace Revo.Infrastructure.EventStores
{
    public class EventStoreEventMessage
    {
        public static IEventMessage FromRecord(IEventStoreRecord record)
        {
            Type messageType = typeof(EventStoreEventMessage<>).MakeGenericType(record.Event.GetType());
            return (IEventMessage)messageType.GetConstructor(new[] { typeof(IEventStoreRecord) })
                .Invoke(new object[] { record });
        }
    }

    public class EventStoreEventMessage<TEvent> : IEventMessage<TEvent>, IEventStoreEventMessage
        where TEvent : IEvent
    {
        public EventStoreEventMessage(IEventStoreRecord record)
        {
            Record = record;

            Metadata = new LayeredMetadata(record.AdditionalMetadata,
                new Dictionary<string, Func<string>>()
                {
                    { BasicEventMetadataNames.EventId, () => record.EventId.ToString() },
                    { BasicEventMetadataNames.StreamSequenceNumber, () => record.StreamSequenceNumber.ToString() },
                    { BasicEventMetadataNames.StoreDate, () => record.StoreDate.ToString(CultureInfo.InvariantCulture) },
                    { BasicEventMetadataNames.PublishDate, () => Clock.Current.UtcNow.ToString(CultureInfo.InvariantCulture) },
                });
        }

        IEvent IEventMessage.Event => Event;

        public IEventStoreRecord Record { get; }
        public TEvent Event => (TEvent)Record.Event;
        public IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
