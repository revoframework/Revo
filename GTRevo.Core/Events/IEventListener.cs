using MediatR;

namespace GTRevo.Core.Events
{
    public interface IEventListener<in T> : IAsyncNotificationHandler<T>
        where T : IEvent
    {
    }
}
