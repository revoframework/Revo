using System;
using GTRevo.DataAccess.Entities;

namespace GTRevo.Infrastructure.EF6.EventSourcing.Model
{
    [TablePrefix(NamespacePrefix = "REV", ColumnPrefix = "DAR")]
    [DatabaseEntity]
    public class DomainAggregateRecord
    {
        public Guid Id { get; set; }
        public Guid ClassId { get; set; }
        //public bool IsDeleted { get; set; }
    }
}
