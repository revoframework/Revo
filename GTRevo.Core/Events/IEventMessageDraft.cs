namespace GTRevo.Core.Events
{
    public interface IEventMessageDraft : IEventMessage
    {
        void SetMetadata(string key, string value);
    }

    public interface IEventMessageDraft<out TEvent> : IEventMessageDraft, IEventMessage<TEvent>
        where TEvent : IEvent
    {
    }
}
