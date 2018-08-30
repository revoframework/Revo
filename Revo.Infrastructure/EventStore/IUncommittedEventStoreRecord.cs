using System.Collections.Generic;
using Revo.Core.Events;

namespace Revo.Infrastructure.EventStore
{
    public interface IUncommittedEventStoreRecord
    {
        IEvent Event { get; }
        IReadOnlyDictionary<string, string> Metadata { get; }
    }
}
