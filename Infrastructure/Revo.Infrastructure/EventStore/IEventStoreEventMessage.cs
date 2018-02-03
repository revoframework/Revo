using Revo.Core.Events;

namespace Revo.Infrastructure.EventStore
{
    public interface IEventStoreEventMessage : IEventMessage
    {
        IEventStoreRecord Record { get; }
    }
}
