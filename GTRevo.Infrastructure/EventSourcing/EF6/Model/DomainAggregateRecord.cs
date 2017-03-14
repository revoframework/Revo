using System;
using GTRevo.DataAccess.EF6;

namespace GTRevo.Infrastructure.EventSourcing.EF6.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "DAR")]
    public class DomainAggregateRecord
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
    }
}
