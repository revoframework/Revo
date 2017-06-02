using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GTRevo.DataAccess.EF6;
using GTRevo.Infrastructure.History.ChangeTracking.Model;
using GTRevo.Platform.Core;
using GTRevo.Platform.Events;
using GTRevo.Platform.Security;

namespace GTRevo.Infrastructure.History.ChangeTracking
{
    public class ChangeTracker : IChangeTracker
    {
        private readonly ICrudRepository crudRepository;
        private readonly IActorContext actorContext;
        private readonly ITrackedChangeRecordConverter trackedChangeRecordConverter;
        private readonly IEventQueue eventQueue;
        private readonly List<TrackedChangeRecord> unsavedChangeRecords = new List<TrackedChangeRecord>();

        public ChangeTracker(ICrudRepository crudRepository,
            IActorContext actorContext,
            ITrackedChangeRecordConverter trackedChangeRecordConverter,
            IEventQueue eventQueue)
        {
            this.crudRepository = crudRepository;
            this.actorContext = actorContext;
            this.trackedChangeRecordConverter = trackedChangeRecordConverter;
            this.eventQueue = eventQueue;
        }

        public TrackedChange AddChange<T>(T changeData, Guid? aggregateId = null, Guid? aggregateClassId = null,
            Guid? entityId = null, Guid? entityClassId = null, Guid? userId = null) where T : ChangeData
        {
            var actorName = actorContext.CurrentActorName;
            var now = Clock.Current.Now;
            TrackedChange change = new TrackedChange(Guid.NewGuid(), changeData, actorName: actorName,
                aggregateId: aggregateId, aggregateClassId: aggregateClassId,
                entityId: entityId, entityClassId: entityClassId, changeTime: now,
                userId: userId);

            TrackedChangeRecord record = trackedChangeRecordConverter.ToRecord(change);
            unsavedChangeRecords.Add(record);

            return change;
        }

        public IQueryable<TrackedChange> FindChanges()
        {
            return crudRepository.FindAll<TrackedChangeRecord>()
                .ProjectToQueryable<TrackedChange>();
        }

        public async Task<TrackedChange> GetChangeAsync(Guid trackedChangeId)
        {
            TrackedChangeRecord record = await crudRepository.GetAsync<TrackedChangeRecord>(trackedChangeId);
            return Mapper.Map<TrackedChange>(record);
        }

        public async Task SaveChangesAsync()
        {
            if (unsavedChangeRecords.Count == 0)
            {
                return;
            }

            var recs = unsavedChangeRecords.ToList();
            unsavedChangeRecords.Clear();

            crudRepository.AddRange(recs);
            await crudRepository.SaveChangesAsync();

            using (var tx = eventQueue.CreateTransaction())
            {
                foreach (TrackedChangeRecord changeRecord in recs)
                {
                    eventQueue.PushEvent(new TrackedChangeAdded()
                    {
                        TrackedChangeId = changeRecord.Id,
                        AggregateClassId = changeRecord.AggregateClassId,
                        AggregateId = changeRecord.AggregateId,
                        EntityClassId = changeRecord.EntityClassId,
                        EntityId = changeRecord.EntityId
                    });
                }
                
                await tx.CommitAsync();
            }
        }
    }
}
