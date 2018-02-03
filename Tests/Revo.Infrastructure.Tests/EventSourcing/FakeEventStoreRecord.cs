using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStore;

namespace Revo.Infrastructure.Tests.EventSourcing
{
    public class FakeEventStoreRecord : IEventStoreRecord
    {
        public IEvent Event { get; set; }
        public IReadOnlyDictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();
        public Guid EventId { get; set; } = Guid.NewGuid();
        public long StreamSequenceNumber { get; set; }
        public DateTimeOffset StoreDate { get; set; } = Clock.Current.Now;
    }
}
