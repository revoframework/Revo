using System;

namespace GTRevo.Infrastructure.Domain
{
    public class DomainAggregateEventRecord
    {
        public DomainAggregateEvent Event { get; set; }
        public string ActorName { get; set; }
        public int AggregateVersion { get; set; }
        public DateTime DatePublished { get; set; }
    }
}