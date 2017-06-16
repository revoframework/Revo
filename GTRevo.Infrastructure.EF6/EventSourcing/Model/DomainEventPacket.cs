using System;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.EF6.EventSourcing.Model
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
