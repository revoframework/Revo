using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.History.ChangeTracking;
using GTRevo.Infrastructure.History.ChangeTracking.Model;
using GTRevo.Testing.Core;
using GTRevo.Testing.DataAccess;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.History.ChangeTracking
{
    public class ChangeTrackerTests
    {
        private readonly ChangeTracker sut;
        private readonly FakeCrudRepository crudRepository;
        private readonly IActorContext actorContext;
        private readonly IEventBus eventBus;
        private readonly ITrackedChangeRecordConverter trackedChangeRecordConverter;
        private readonly Guid aggregateId = Guid.NewGuid();
        private readonly Guid aggregateClassId = Guid.NewGuid();
        private readonly Guid entityId = Guid.NewGuid();
        private readonly Guid entityClassId = Guid.NewGuid();

        public ChangeTrackerTests()
        {
            crudRepository = new FakeCrudRepository();
            actorContext = Substitute.For<IActorContext>();
            actorContext.CurrentActorName.Returns("actor");
            eventBus = Substitute.For<IEventBus>();
            FakeClock.Setup();
            FakeClock.Now = DateTime.Today;
            trackedChangeRecordConverter = Substitute.For<ITrackedChangeRecordConverter>();

            Mapper.Initialize(x => new HistoryAutoMapperDefinition(trackedChangeRecordConverter).Configure(x));

            sut = new ChangeTracker(crudRepository, actorContext, trackedChangeRecordConverter, eventBus);
        }

        [Fact]
        public void AddChange_ReturnsChange()
        {
            TestChangeData changeData = new TestChangeData();

            TrackedChange change = sut.AddChange(changeData, aggregateId, aggregateClassId,
                entityId, entityClassId);
            trackedChangeRecordConverter.ToRecord(null).ReturnsForAnyArgs(
                new TrackedChangeRecord());

            Assert.Equal(aggregateId, change.AggregateId);
            Assert.Equal(aggregateClassId, change.AggregateClassId);
            Assert.Equal(entityId, change.EntityId);
            Assert.Equal(entityClassId, change.EntityClassId);
            Assert.Equal(changeData, change.ChangeData);
            Assert.Equal(actorContext.CurrentActorName, change.ActorName);
            Assert.Equal(FakeClock.Now, change.ChangeTime);
        }

        [Fact]
        public async Task AddChange_And_SaveChanges_AddsRecordToRepository()
        {
            TestChangeData changeData = new TestChangeData();

            TrackedChangeRecord record = new TrackedChangeRecord();
            trackedChangeRecordConverter.ToRecord(null).ReturnsForAnyArgs(record);
            
            sut.AddChange(changeData, aggregateId, aggregateClassId,
                entityId, entityClassId);
            await sut.SaveChangesAsync();

            Assert.Equal(1, crudRepository.FindAll<TrackedChangeRecord>().Count(x => x == record));
        }

        [Fact]
        public async Task AddChange_And_SaveChanges_PublishesEvents()
        {
            TestChangeData changeData = new TestChangeData();
            
            trackedChangeRecordConverter.ToRecord(null).ReturnsForAnyArgs(ci =>
            {
                TrackedChange x = ci.ArgAt<TrackedChange>(0);
                return new TrackedChangeRecord()
                {
                    Id = x.Id,
                    AggregateId = x.AggregateId,
                    AggregateClassId = x.AggregateClassId,
                    EntityId = x.EntityId,
                    EntityClassId = x.EntityClassId
                };
            });
            
            TrackedChange tc = sut.AddChange(changeData, aggregateId, aggregateClassId,
                entityId, entityClassId);
            await sut.SaveChangesAsync();

            eventBus.Received(1).PublishAsync(Arg.Is<IEventMessage<TrackedChangeAdded>>(
                x => x.Event.AggregateId == aggregateId
                     && x.Event.EntityClassId == entityClassId
                     && x.Event.EntityId == entityId
                     && x.Event.TrackedChangeId == tc.Id));
        }

        [Fact]
        public void FindChanges_ReturnsChanges()
        {
            TrackedChangeRecord record = new TrackedChangeRecord();
            TrackedChange change = new TrackedChange(Guid.NewGuid(), new TestChangeData(),
                "", null, null, null, null, null, DateTimeOffset.Now);

            trackedChangeRecordConverter.FromRecord(record).Returns(change);
            crudRepository.Attach(record);
            List<TrackedChange> changes = sut.FindChanges().ToList();

            Assert.Equal(1, changes.Count);
            Assert.Contains(change, changes);
        }

        [Fact]
        public async Task GetChangeAsync_ReturnsChange()
        {
            TrackedChangeRecord record = new TrackedChangeRecord() { Id = Guid.NewGuid() };
            TrackedChange change = new TrackedChange(record.Id, new TestChangeData(),
                "", null, null, null, null, null, DateTimeOffset.Now);

            trackedChangeRecordConverter.FromRecord(record).Returns(change);
            crudRepository.Attach(record);
            TrackedChange change2 = await sut.GetChangeAsync(change.Id);

            Assert.Equal(change, change2);
        }

        public class TestChangeData : ChangeData
        {
        }
    }
}
