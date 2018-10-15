using System;
using Revo.DataAccess.Entities;
using Revo.Domain.ReadModel;

namespace Revo.Extensions.History.ChangeTracking.Model
{
    [TablePrefix(NamespacePrefix = "RHI", ColumnPrefix = "TCH")]
    public class TrackedChangeRecord : ReadModelBase
    {
        public Guid Id { get; set; }
        public string ActorName { get; set; }
        public Guid? UserId { get; set; }
        public Guid? AggregateId { get; set; }
        public Guid? AggregateClassId { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? EntityClassId { get; set; }
        public DateTimeOffset ChangeTime { get; set; }
        
        public string ChangeDataJson { get; set; }
        public string ChangeDataClassName { get; set; }
    }
}
