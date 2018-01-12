using System.Collections.Generic;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.EventStore
{
    public interface IUncommittedEventStoreRecord
    {
        IEvent Event { get; }
        IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
