using System;
using System.Threading.Tasks;
using GTRevo.Infrastructure.History.ChangeTracking;
using GTRevo.Infrastructure.History.ChangeTracking.Changes;
using GTRevo.Testing.DataAccess.EF6;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.History.ChangeTracking
{
    public class EntityAttributeChangeLoggerTests
    {
        private readonly FakeCrudRepository crudRepository;
        private readonly IChangeTracker changeTracker;
        private readonly EntityAttributeChangeLogger sut;

        private readonly Guid aggregateId = Guid.NewGuid();
        private readonly Guid aggregateClassId = Guid.NewGuid();
        private readonly Guid entityId = Guid.NewGuid();
        private readonly Guid entityClassId = Guid.NewGuid();

        public EntityAttributeChangeLoggerTests()
        {
            crudRepository = new FakeCrudRepository();
            changeTracker = Substitute.For<IChangeTracker>();
            sut = new EntityAttributeChangeLogger(changeTracker, crudRepository);
        }

        [Fact]
        public void Batch_ReturnsBatchInstance()
        {
            Assert.IsType<EntityAttributeChangeBatch>(sut.Batch(null, null, null, null));
            // TODO check IDs
        }

        [Fact]
        public async Task ChangeAttribute_AddChanges()
        {
            await sut.ChangeAttribute("Foo", "bar", aggregateId, aggregateClassId,
                entityId, entityClassId);
            await sut.ChangeAttribute("Foo", "moo", aggregateId, aggregateClassId,
                entityId, entityClassId);

            changeTracker.ReceivedWithAnyArgs(1).AddChange<EntityAttributeChangeData<string>>(
                null, null, null, null, null);

            changeTracker.Received(1).AddChange(
                Arg.Is<EntityAttributeChangeData<string>>(
                    x => x.AttributeName == "Foo"
                         && x.NewValue == "moo"
                         && x.OldValue == "bar"),
                aggregateId, aggregateClassId, entityId, entityClassId);
        }

        [Fact]
        public async Task ChangeAttribute_DoesntAddChangesWhenValuesEqual()
        {
            await sut.ChangeAttribute("Foo", "bar", aggregateId, aggregateClassId,
                entityId, entityClassId);
            await sut.ChangeAttribute("Foo", "moo", aggregateId, aggregateClassId,
                entityId, entityClassId);
            await sut.ChangeAttribute("Foo", "moo", aggregateId, aggregateClassId,
                entityId, entityClassId);

            changeTracker.ReceivedWithAnyArgs(1).AddChange<EntityAttributeChangeData<string>>(
                null, null, null, null, null);
        }

        [Fact]
        public void ChangeAttribute_StoresEntityAttributeValues()
        {
            ///
        }
    }
}
