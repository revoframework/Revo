using System;
using System.Collections.Generic;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.EventStores;

namespace Revo.Testing.Infrastructure
{
    public class FakeEventStoreRecord : IEventStoreRecord
    {
        public IEvent Event { get; set; }
        public IReadOnlyDictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();
        public Guid EventId { get; set; } = Guid.NewGuid();
        public long StreamSequenceNumber { get; set; }
        public DateTimeOffset StoreDate { get; set; } = Clock.Current.UtcNow;
    }
}
