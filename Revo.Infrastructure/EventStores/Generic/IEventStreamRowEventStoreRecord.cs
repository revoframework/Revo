using Revo.Infrastructure.EventStores.Generic.Model;

namespace Revo.Infrastructure.EventStores.Generic
{
    public interface IEventStreamRowEventStoreRecord : IEventStoreRecord
    {
        EventStreamRow EventStreamRow { get; }
    }
}