using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.Events
{
    public interface IEventMessageDraft : IEventMessage
    {
        void AddMetadata(string key, string value);
    }

    public interface IEventMessageDraft<out TEvent> : IEventMessageDraft, IEventMessage<TEvent>
        where TEvent : IEvent
    {
    }
}
