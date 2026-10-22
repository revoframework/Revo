using System;
using System.Collections.Generic;

namespace Revo.Core.Events
{
    public static class EventMessage
    {
        public static IEventMessage FromEvent(IEvent @event, IReadOnlyDictionary<string, string> metadata)
        {
            Type messageType = typeof(EventMessage<>).MakeGenericType(@event.GetType());

            return (IEventMessage)messageType.GetConstructor(new[]
                    {@event.GetType(), typeof(IReadOnlyDictionary<string, string>)})
                .Invoke(new object[] { @event, metadata });
        }
    }

    public class EventMessage<TEvent>(TEvent @event, IReadOnlyDictionary<string, string> metadata) : IEventMessage<TEvent>
        where TEvent : IEvent
    {
        IEvent IEventMessage.Event => Event;

        public TEvent Event { get; } = @event;
        public IReadOnlyDictionary<string, string> Metadata { get; } = metadata;
    }
}
