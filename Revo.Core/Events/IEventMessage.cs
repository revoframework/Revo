using System.Collections.Generic;

namespace Revo.Core.Events
{
    public interface IEventMessage
    {
        IEvent Event { get; }
        IReadOnlyDictionary<string, string> Metadata { get; }
    }

    public interface IEventMessage<out TEvent> : IEventMessage
        where TEvent : IEvent
    {
        new TEvent Event { get; }
    }
}
