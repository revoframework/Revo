using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Domain;
using GTRevo.Infrastructure.Domain.Events;

namespace GTRevo.Infrastructure.EventSourcing.EF6
{
    public class AggregateHistory : IAggregateHistory
    {
        private readonly Func<Task<IEnumerable<DomainAggregateEventRecord>>> events;

        public AggregateHistory(Guid aggregateId, Guid aggregateClassId,
            int version, Func<Task<IEnumerable<DomainAggregateEventRecord>>> events)
        {
            AggregateId = aggregateId;
            AggregateClassId = aggregateClassId;
            Version = version;
            this.events = events;
        }
        
        public Guid AggregateId { get; private set; }
        public Guid AggregateClassId { get; private set; }
        public int Version { get; private set; }

        public Task<IEnumerable<DomainAggregateEventRecord>> GetEventsAsync()
        {
            return events();
        }
    }
}
