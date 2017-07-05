using System;
using GTRevo.Core.Events;

namespace GTRevo.Infrastructure.History.ChangeTracking
{
    public class TrackedChangeAdded : IEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TrackedChangeId { get; set; }
        public Guid? AggregateId { get; set; }
        public Guid? AggregateClassId { get; set; }
        public Guid? EntityId { get; set; }
        public Guid? EntityClassId { get; set; }
    }
}
