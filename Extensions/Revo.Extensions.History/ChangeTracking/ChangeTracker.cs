using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.Extensions.History.ChangeTracking.Model;

namespace Revo.Extensions.History.ChangeTracking
{
    public class ChangeTracker : IChangeTracker
    {
        private readonly ICrudRepository crudRepository;
        private readonly IActorContext actorContext;
        private readonly ITrackedChangeRecordConverter trackedChangeRecordConverter;
        private readonly IEventBus eventBus;
        private readonly List<TrackedChangeRecord> unsavedChangeRecords = new List<TrackedChangeRecord>();
        private readonly IMapper mapper;

        public ChangeTracker(
            ICrudRepository crudRepository,
            IActorContext actorContext,
            ITrackedChangeRecordConverter trackedChangeRecordConverter,
            IEventBus eventBus,
            IMapper mapper)
        {
            this.crudRepository = crudRepository;
            this.actorContext = actorContext;
            this.trackedChangeRecordConverter = trackedChangeRecordConverter;
            this.eventBus = eventBus;
            this.mapper = mapper;
        }

        public TrackedChange AddChange<T>(T changeData, Guid? aggregateId = null, Guid? aggregateClassId = null,
            Guid? entityId = null, Guid? entityClassId = null, Guid? userId = null) where T : ChangeData
        {
            var actorName = actorContext.CurrentActorName;
            var now = Clock.Current.UtcNow;
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
            return crudRepository
                .FindAll<TrackedChangeRecord>()
                .UseAsDataSource(mapper)
                .For<TrackedChange>();
        }

        public async Task<TrackedChange> GetChangeAsync(Guid trackedChangeId)
        {
            TrackedChangeRecord record = await crudRepository.GetAsync<TrackedChangeRecord>(trackedChangeId);
            return mapper.Map<TrackedChange>(record);
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

            foreach (TrackedChangeRecord changeRecord in recs)
            {
                await eventBus.PublishAsync(new EventMessage<TrackedChangeAdded>(
                    new TrackedChangeAdded()
                    {
                        TrackedChangeId = changeRecord.Id,
                        AggregateClassId = changeRecord.AggregateClassId,
                        AggregateId = changeRecord.AggregateId,
                        EntityClassId = changeRecord.EntityClassId,
                        EntityId = changeRecord.EntityId
                    },
                    new Dictionary<string, string>()));
            }
        }
    }
}
