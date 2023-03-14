using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.DataAccess.Entities;
using Revo.DataAccess.InMemory;
using Revo.Domain.Entities;
using Revo.Domain.Entities.Attributes;
using Revo.Domain.Entities.Basic;
using Revo.Domain.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.Repositories;
using Revo.Testing.Core;
using Xunit;

namespace Revo.Infrastructure.Tests.Repositories
{
    public class CrudAggregateStoreTests
    {
        private const string TestAggregateClassIdString = "{4057D3AC-2D96-4C25-935D-F72BAC6BA626}";
        private static readonly Guid TestAggregateClassId = Guid.Parse(TestAggregateClassIdString);

        private readonly CrudAggregateStore sut;
        private readonly InMemoryCrudRepository crudRepository;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventMessageFactory eventMessageFactory;
        private readonly List<DomainClassInfo> domainClasses;

        public CrudAggregateStoreTests()
        {
            crudRepository = Substitute.ForPartsOf<InMemoryCrudRepository>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();

            domainClasses = new List<DomainClassInfo>()
            {
                new DomainClassInfo(TestAggregateClassId, null, typeof(TestAggregate))
            };

            entityTypeManager.GetClassInfoByClassId(Guid.Empty)
                .ReturnsForAnyArgs(ci => domainClasses.Single(x => x.Id == ci.Arg<Guid>()));
            entityTypeManager.TryGetClassInfoByClrType(null)
                .ReturnsForAnyArgs(ci => domainClasses.SingleOrDefault(x => x.ClrType == ci.Arg<Type>()));
            entityTypeManager.GetClassInfoByClrType(null)
                .ReturnsForAnyArgs(ci => domainClasses.Single(x => x.ClrType == ci.Arg<Type>()));

            eventMessageFactory = Substitute.For<IEventMessageFactory>();
            eventMessageFactory.CreateMessageAsync(null).ReturnsForAnyArgs(ci =>
            {
                var @event = ci.ArgAt<IEvent>(0);
                Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
                IEventMessageDraft messageDraft = (IEventMessageDraft)messageType.GetConstructor(new[] { @event.GetType() }).Invoke(new[] { @event });
                messageDraft.SetMetadata("TestKey", "TestValue");
                return messageDraft;
            }); // TODO something more lightweight?

            FakeClock.Setup();

            sut = new CrudAggregateStore(crudRepository, entityTypeManager, publishEventBuffer, eventMessageFactory);
        }

        [Fact]
        public async Task GetManyAsync_FindsCorrectEntities()
        {
            var entity1 = new TestAggregate(Guid.NewGuid());
            var entity2 = new TestAggregate(Guid.NewGuid());
            crudRepository.AttachRange(new[] { entity1, entity2 });

            var result = await sut.GetManyAsync<TestAggregate>(entity1.Id, entity2.Id);
            result.Should().BeEquivalentTo(new[] { entity1, entity2 });
        }

        [Fact]
        public async Task SaveChanges_InjectsClassIds()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);

            await sut.SaveChangesAsync();

            Assert.Equal(TestAggregateClassId, testAggregate.ClassId);
        }

        [Fact]
        public async Task SaveChanges_PushesEventsForPublishing()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);
            testAggregate.Do();
            var event1 = testAggregate.UncommittedEvents.First();

            await sut.SaveChangesAsync();

            publishEventBuffer.Received(1).PushEvent(Arg.Is<IEventMessage>(x => x.Event == event1));
        }

        [Fact]
        public async Task SaveChanges_PublishedEventsHaveMetadata()
        {
            FakeClock.Now = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
            
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            sut.Add(testAggregate);
            testAggregate.Do();
            var event1 = testAggregate.UncommittedEvents.First();

            List<IEventMessage> publishedMessages = new List<IEventMessage>();
            publishEventBuffer.WhenForAnyArgs(x => x.PushEvent(null))
                .Do(ci => publishedMessages.Add(ci.ArgAt<IEventMessage>(0)));

            await sut.SaveChangesAsync();

            publishedMessages.Should().HaveCount(1);
            publishedMessages[0].Event.Should().Be(event1);
            publishedMessages[0].Metadata.Should().Contain(x => x.Key == BasicEventMetadataNames.EventId
                                                                && Guid.Parse(x.Value) != Guid.Empty);
            publishedMessages[0].Metadata.Should().Contain(x => x.Key == BasicEventMetadataNames.AggregateVersion
                                                                && int.Parse(x.Value) == 1);
            publishedMessages[0].Metadata.Should().Contain(x => x.Key == BasicEventMetadataNames.AggregateClassId
                                                                && Guid.Parse(x.Value) == Guid.Parse(TestAggregateClassIdString));
            publishedMessages[0].Metadata.Should().Contain(x => x.Key == BasicEventMetadataNames.StoreDate
                                                                && DateTimeOffset.Parse(x.Value) == FakeClock.Now);
            // TODO TenantId
        }
        
        [Fact]
        public async Task SaveChanges_CommitsAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(Guid.NewGuid());
            domainClasses.Add(new DomainClassInfo(TestAggregateClassId, null, testAggregate.GetType()));

            sut.Add(testAggregate);
            testAggregate.Do();

            await sut.SaveChangesAsync();

            testAggregate.Received(1).Commit();
        }
        
        [Fact]
        public async Task SaveChanges_CommitsOnlyChangedAggregates()
        {
            TestAggregate testAggregate = Substitute.ForPartsOf<TestAggregate>(Guid.NewGuid());
            domainClasses.Add(new DomainClassInfo(TestAggregateClassId, null, testAggregate.GetType()));

            sut.Add(testAggregate);

            await sut.SaveChangesAsync();

            testAggregate.Received(0).Commit();
        }

        [Fact]
        public async Task SaveChanges_RemovesDeleted()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            crudRepository.Add(testAggregate);
            await crudRepository.SaveChangesAsync();

            testAggregate = await sut.GetAsync<TestAggregate>(testAggregate.Id);
            testAggregate.Do();
            var event1 = testAggregate.UncommittedEvents.First();
            testAggregate.Delete();

            await sut.SaveChangesAsync();

            Received.InOrder(() =>
            {
                crudRepository.SaveChangesAsync();
                crudRepository.Remove(testAggregate);
                crudRepository.SaveChangesAsync();
            });

            publishEventBuffer.Received(1).PushEvent(Arg.Is<IEventMessage>(x => x.Event == event1));
            crudRepository.GetEntityState(testAggregate).Should().Be(EntityState.Detached);
        }

        [Fact]
        public async Task SaveChanges_RemovesDeletedAndDependencies()
        {
            // some providers like EF Core might remove other related entities (e.g. owned collections) when removing an aggregate,
            // modifying the internal collection - RemoveDeletedEntities needs to materialize the iterated collection of to-be-deleted aggregates first

            var testAggregate = new TestAggregate(Guid.NewGuid());
            crudRepository.Add(testAggregate);

            var testEntity = new TestEntity(Guid.NewGuid());
            crudRepository.Add(testEntity);
            await crudRepository.SaveChangesAsync();

            crudRepository.When(x => x.Remove(testAggregate))
                .Do(ci =>
                {
                    crudRepository.Remove(testEntity);
                    crudRepository.SaveChanges();
                });

            testAggregate = await sut.GetAsync<TestAggregate>(testAggregate.Id);
            testAggregate.Delete();

            await sut.SaveChangesAsync();
            
            crudRepository.GetEntityState(testAggregate).Should().Be(EntityState.Detached);
            crudRepository.GetEntityState(testEntity).Should().Be(EntityState.Detached);
        }

        [Fact]
        public async Task SaveChanges_FindNull()
        {
            var result = await sut.FindAsync<TestAggregate>(Guid.NewGuid());
            result.Should().BeNull();
        }

        [Fact]
        public async Task SaveChanges_GetThrows()
        {
            var result = await sut.Awaiting(x => x.GetAsync<TestAggregate>(Guid.NewGuid()))
                .Should().ThrowAsync<EntityNotFoundException>();
        }

        [Fact]
        public async Task SaveChanges_FindAsyncDeleted()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            crudRepository.Add(testAggregate);
            await crudRepository.SaveChangesAsync();

            testAggregate = await sut.GetAsync<TestAggregate>(testAggregate.Id);
            testAggregate.Delete();
            var testAggregate2 = await sut.FindAsync<TestAggregate>(testAggregate.Id);
            testAggregate2.Should().BeNull();
        }

        [Fact]
        public async Task SaveChanges_FindManyAsyncDeleted()
        {
            TestAggregate testAggregate1 = new TestAggregate(Guid.NewGuid());
            TestAggregate testAggregate2 = new TestAggregate(Guid.NewGuid());
            crudRepository.AddRange(new[] { testAggregate1, testAggregate2 });
            await crudRepository.SaveChangesAsync();

            (await sut.GetAsync<TestAggregate>(testAggregate2.Id)).Delete();
            var result = await sut.FindManyAsync<TestAggregate>(testAggregate1.Id, testAggregate2.Id);
            result.Should().BeEquivalentTo(new[] {testAggregate1});
        }

        [Fact]
        public async Task SaveChanges_GetAsyncDeleted()
        {
            TestAggregate testAggregate = new TestAggregate(Guid.NewGuid());
            crudRepository.Add(testAggregate);
            await crudRepository.SaveChangesAsync();

            testAggregate = await sut.GetAsync<TestAggregate>(testAggregate.Id);
            testAggregate.Delete();
            Func<Task> act = async () => await sut.GetAsync<TestAggregate>(testAggregate.Id);
            await act.Should().ThrowAsync<EntityDeletedException>();
        }

        [Fact]
        public async Task SaveChanges_GetManyAsyncDeleted()
        {
            TestAggregate testAggregate1 = new TestAggregate(Guid.NewGuid());
            TestAggregate testAggregate2 = new TestAggregate(Guid.NewGuid());
            crudRepository.AddRange(new[] { testAggregate1, testAggregate2 });
            await crudRepository.SaveChangesAsync();
            
            (await sut.GetAsync<TestAggregate>(testAggregate2.Id)).Delete();
            Func<Task> act = async () => await sut.GetManyAsync<TestAggregate>(testAggregate1.Id, testAggregate2.Id);
            await act.Should().ThrowAsync<EntityDeletedException>();
        }

        [DomainClassId(TestAggregateClassIdString)]
        public class TestAggregate : BasicClassAggregateRoot
        {
            public TestAggregate(Guid id) : base(id)
            {
            }

            protected TestAggregate()
            {
            }

            public void Do()
            {
                Publish(new Event1());
            }

            public void Delete()
            {
                MarkDeleted();
            }
        }

        public class TestEntity : BasicEntity
        {
            public TestEntity(Guid id) : base(id)
            {
            }

            protected TestEntity()
            {
            }
        }

        public class Event1 : DomainAggregateEvent
        {
        }
    }
}
