using System;

namespace Revo.Extensions.History.ChangeTracking
{
    public class TrackedChange
    {
        public TrackedChange(Guid id, ChangeData changeData, string actorName, Guid? userId,
            Guid? aggregateId, Guid? aggregateClassId, Guid? entityId, Guid? entityClassId,
            DateTimeOffset changeTime)
        {
            Id = id;
            ActorName = actorName;
            UserId = userId;
            AggregateId = aggregateId;
            AggregateClassId = aggregateClassId;
            ChangeData = changeData;
            EntityId = entityId;
            EntityClassId = entityClassId;
            ChangeTime = changeTime;
        }

        public Guid Id { get; private set; }
        public string ActorName { get; private set; }
        public Guid? UserId { get; private set; }
        public Guid? AggregateId { get; private set; }
        public Guid? AggregateClassId { get; private set; }
        public ChangeData ChangeData { get; private set; }
        public Guid? EntityId { get; private set; }
        public Guid? EntityClassId { get; private set; }
        public DateTimeOffset ChangeTime { get; private set; }

        public T ChangeDataAs<T>() where T : ChangeData
        {
            return (T)ChangeData;
        }
    }
}
