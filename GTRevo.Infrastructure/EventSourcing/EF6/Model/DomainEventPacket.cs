using System;
using GTRevo.DataAccess.EF6;

namespace GTRevo.Infrastructure.EventSourcing.EF6.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "DEP")]
    [DatabaseEntity]
    public class DomainEventPacket
    {
        public Guid Id { get; set; }
        public Guid AggregateId { get; set; }
        public int SequenceNumber { get; set; }
        public string ActorName { get; set; }
        public DateTime DatePublished { get; set; }
        public string EventsJson { get; set; }
    }
}
