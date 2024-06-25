using System;
using Newtonsoft.Json;
using Revo.Extensions.History.ChangeTracking.Model;

namespace Revo.Extensions.History.ChangeTracking
{
    public class TrackedChangeRecordConverter : ITrackedChangeRecordConverter
    {
        private readonly IChangeDataTypeCache changeDataTypeCache;

        public TrackedChangeRecordConverter(IChangeDataTypeCache changeDataTypeCache)
        {
            this.changeDataTypeCache = changeDataTypeCache;
        }

        public TrackedChange FromRecord(TrackedChangeRecord record)
        {
            Type changeDataType = changeDataTypeCache.GetClrChangeDataType(record.ChangeDataClassName);
            ChangeData changeData = (ChangeData)JsonConvert.DeserializeObject(record.ChangeDataJson, changeDataType);

            return new TrackedChange(
                record.Id,
                changeData,
                actorName: record.ActorName,
                userId: record.UserId,
                aggregateClassId: record.AggregateClassId,
                aggregateId: record.AggregateId,
                entityClassId: record.EntityClassId,
                entityId: record.EntityId,
                changeTime: record.ChangeTime
            );
        }

        public TrackedChangeRecord ToRecord(TrackedChange change)
        {
            return new TrackedChangeRecord()
            {
                Id = change.Id,
                ActorName = change.ActorName,
                UserId = change.UserId,
                AggregateClassId = change.AggregateClassId,
                AggregateId = change.AggregateId,
                ChangeDataClassName = changeDataTypeCache.GetChangeDataTypeName(change.ChangeData.GetType()),
                ChangeDataJson = JsonConvert.SerializeObject(change.ChangeData),
                ChangeTime = change.ChangeTime,
                EntityClassId = change.EntityClassId,
                EntityId = change.EntityId
            };
        }
    }
}
