using MediatR;

namespace GTRevo.Platform.Events
{
    public interface IEventListener<in T> : IAsyncNotificationHandler<T>
        where T : IEvent
    {
    }
}
