using MediatR;

namespace GTRevo.Events
{
    public interface IEventListener<in T> : IAsyncNotificationHandler<T>
        where T : IEvent
    {
    }
}
