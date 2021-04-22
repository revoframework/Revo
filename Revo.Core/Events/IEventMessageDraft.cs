namespace Revo.Core.Events
{
    public interface IEventMessageDraft : IEventMessage
    {
        IEventMessageDraft SetMetadata(string key, string value);
    }

    public interface IEventMessageDraft<out TEvent> : IEventMessageDraft, IEventMessage<TEvent>
        where TEvent : IEvent
    {
    }
}
