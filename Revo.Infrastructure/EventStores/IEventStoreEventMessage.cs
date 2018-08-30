using Revo.Core.Events;

namespace Revo.Infrastructure.EventStores
{
    public interface IEventStoreEventMessage : IEventMessage
    {
        IEventStoreRecord Record { get; }
    }
}
