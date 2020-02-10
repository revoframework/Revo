using System;
using System.Collections.Generic;
using MoreLinq;
using Revo.Core.Events;

namespace Revo.Infrastructure.Events.Upgrades
{
    public abstract class UpgradedEventMessage : IEventMessage
    {
        public static UpgradedEventMessage Create(IEventMessage<IEvent> originalMessage,
            IEvent newEvent, params KeyValuePair<string, Func<string>>[] metadataOverrides)
        {
            Type messageType = typeof(UpgradedEventMessage<>).MakeGenericType(newEvent.GetType());
            return (UpgradedEventMessage)messageType.GetConstructor(new[] { typeof(IEventMessage<IEvent>),
                    newEvent.GetType(), typeof(KeyValuePair<string, Func<string>>[]) })
                .Invoke(new object[] { originalMessage, newEvent, metadataOverrides });
        }

        public IEvent Event => UntypedEvent;
        public abstract IReadOnlyDictionary<string, string> Metadata { get; }
        public Dictionary<string, Func<string>> MetadataOverrides { get; set; }
        protected abstract IEvent UntypedEvent { get; }
    }

    public class UpgradedEventMessage<T> : UpgradedEventMessage, IEventMessage<T>
        where T : IEvent
    {
        IEvent IEventMessage.Event => Event;

        public UpgradedEventMessage(IEventMessage<IEvent> originalMessage,
            T newEvent,
            params KeyValuePair<string, Func<string>>[] metadataOverrides)
        {
            Event = newEvent;
            MetadataOverrides = metadataOverrides?.ToDictionary() ?? new Dictionary<string, Func<string>>();
            Metadata = new LayeredMetadata(originalMessage.Metadata, MetadataOverrides);
        }

        public new T Event { get; }
        public override IReadOnlyDictionary<string, string> Metadata { get; }
        protected override IEvent UntypedEvent => Event;
    }
}