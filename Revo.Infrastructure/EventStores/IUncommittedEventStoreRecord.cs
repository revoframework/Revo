using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.Infrastructure.EventStores
{
    public interface IUncommittedEventStoreRecord
    {
        IEvent Event { get; }
        IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
