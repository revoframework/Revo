using System;

namespace GTRevo.Infrastructure.Domain.Events
{
    public class DomainAggregateEventRecord
    {
        public DomainAggregateEvent Event { get; set; }
        public string ActorName { get; set; }
        public int AggregateVersion { get; set; }
        public DateTime DatePublished { get; set; }

        public override bool Equals(object obj)
        {
            DomainAggregateEventRecord other = obj as DomainAggregateEventRecord;
            if (other == null)
            {
                return false;
            }

            return Event == other.Event
                       && ActorName == other.ActorName
                       && AggregateVersion == other.AggregateVersion
                       && DatePublished == other.DatePublished;
        }
    }
}