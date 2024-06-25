using System;

namespace Revo.Extensions.History.ChangeTracking
{
    public class TrackedChange(Guid id, ChangeData changeData, string actorName, Guid? userId,
            Guid? aggregateId, Guid? aggregateClassId, Guid? entityId, Guid? entityClassId,
            DateTimeOffset changeTime)
    {
        public Guid Id { get; private set; } = id;
        public string ActorName { get; private set; }= actorName;
        public Guid? UserId { get; private set; }= userId;
        public Guid? AggregateId { get; private set; }= aggregateId;
        public Guid? AggregateClassId { get; private set; }= entityClassId;
        public ChangeData ChangeData { get; private set; }= changeData;
        public Guid? EntityId { get; private set; } = entityId;
        public Guid? EntityClassId { get; private set; }= entityClassId;
        public DateTimeOffset ChangeTime { get; private set; } = changeTime;

        public T ChangeDataAs<T>() where T : ChangeData
        {
            return (T)ChangeData;
        }
    }
}
