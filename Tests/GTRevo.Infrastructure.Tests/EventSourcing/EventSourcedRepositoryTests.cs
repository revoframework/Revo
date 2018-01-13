using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.Events;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Infrastructure.EventStore;
using GTRevo.Testing.Core;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.EventSourcing
{
    public class EventSourcedRepositoryTests
    {
        private readonly IPublishEventBuffer publishEventBuffer;
        private readonly IEventStore eventStore;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IRepositoryFilter repositoryFilter1;
        private readonly IRepositoryFilter repositoryFilter2;
        private readonly IEventMessageFactory eventMessageFactory;

        private Guid entityId = Guid.NewGuid();
        private Guid entity2Id = Guid.NewGuid();
        private Guid entity3Id = Guid.NewGuid();
        private Guid entityClassId = Guid.NewGuid();
        private Guid entity2ClassId = Guid.NewGuid();
        private Guid entity3ClassId = Guid.NewGuid();

        private EventSourcedAggregateRepository sut;

        public EventSourcedRepositoryTests()
        {
            publishEventBuffer = Substitute.For<IPublishEventBuffer>();
            eventStore = Substitute.For<IEventStore>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            repositoryFilter1 = Substitute.For<IRepositoryFilter>();
            repositoryFilter2 = Substitute.For<IRepositoryFilter>();
            
            FakeClock.Setup();

            eventStore.GetEventsAsync(entity2Id)
                .Returns(new List<IEventStoreRecord>()
                {
                    new FakeEventStoreRecord()
                    {
                        Event = new SetFooEvent()
                        {
                            AggregateId = entity2Id
                        },
                        StreamSequenceNumber = 1
                    }
                });

            eventStore.GetStreamMetadataAsync(entity2Id)
                .Returns(new Dictionary<string, string>()
                {
                    { "TestKey", "TestValue" },
                    { BasicEventMetadataNames.AggregateClassId, entity2ClassId.ToString() }
                });

            eventStore.GetStreamMetadataAsync(entity3Id)
                .Returns(new Dictionary<string, string>()
                {
                    { "TestKey", "TestValue" },
                    { BasicEventMetadataNames.AggregateClassId, entity3ClassId.ToString() }
                });

            entityTypeManager.GetClrTypeByClassId(entityClassId)
                .Returns(typeof(MyEntity));
            entityTypeManager.GetClrTypeByClassId(entity2ClassId)
                .Returns(typeof(MyEntity2));
            entityTypeManager.GetClrTypeByClassId(entity3ClassId)
                .Returns(typeof(MyEntity3LoadsAsDeleted));

            entityTypeManager.TryGetClassIdByClrType(typeof(MyEntity))
                .Returns(entityClassId);
            entityTypeManager.TryGetClassIdByClrType(typeof(MyEntity2))
                .Returns(entity2ClassId);
            entityTypeManager.TryGetClassIdByClrType(typeof(MyEntity3LoadsAsDeleted))
                .Returns(entity3ClassId);

            eventMessageFactory = Substitute.For<IEventMessageFactory>();
            eventMessageFactory.CreateMessageAsync(null).ReturnsForAnyArgs(ci =>
            {
                var @event = ci.ArgAt<IEvent>(0);
                Type messageType = typeof(EventMessageDraft<>).MakeGenericType(@event.GetType());
                IEventMessageDraft messageDraft = (IEventMessageDraft)messageType.GetConstructor(new[] { @event.GetType() }).Invoke(new[] { @event });
                messageDraft.SetMetadata("TestKey", "TestValue");
                return messageDraft;
            }); // TODO something more lightweight?

            sut = new EventSourcedAggregateRepository(eventStore,
                entityTypeManager, publishEventBuffer, new IRepositoryFilter[] {}, eventMessageFactory);
        }

        [Fact]
        public async Task GetAsync_ReturnsCorrectAggregate()
        {
            var entity = await sut.GetAsync<MyEntity2>(entity2Id);

            Assert.Equal(entity2Id, entity.Id);

            Assert.Equal(1, entity.LoadedEvents.Count);
            Assert.IsType<SetFooEvent>(entity.LoadedEvents[0]);
            Assert.Equal(1, entity.Version);
        }

        [Fact]
        public async Task GetAsync_CachesReturnedEntities()
        {
            var entity1 = await sut.GetAsync<MyEntity2>(entity2Id);
            var entity2 = await sut.GetAsync<MyEntity2>(entity2Id);

            Assert.NotNull(entity1);
            Assert.Equal(entity1, entity2);
        }

        [Fact]
        public async Task GetAsync_ReturnsCorrectDescendant()
        {
            var entity = await sut.GetAsync<MyEntity>(entity2Id);
            Assert.IsType<MyEntity2>(entity);
        }

        [Fact]
        public async Task GetAsync_ThrowsIfDeleted()
        {
            eventStore.GetEventsAsync(entity3Id)
                .Returns(new List<IEventStoreRecord>()
                {
                    new FakeEventStoreRecord()
                    {
                        Event = new SetFooEvent()
                        {
                            AggregateId = entityId
                        },
                        StreamSequenceNumber = 1
                    }
                });

            await Assert.ThrowsAsync<EntityDeletedException>(async () =>
            {
                await sut.GetAsync<MyEntity3LoadsAsDeleted>(entity3Id);
            });
        }

        [Fact]
        public async Task GetAsync_ThrowsIfDeletedAfterLoading()
        {
            var entity = await sut.GetAsync<MyEntity2>(entity2Id);
            entity.IsDeleted = true;

            await Assert.ThrowsAsync<EntityDeletedException>(async () =>
            {
                await sut.GetAsync<MyEntity2>(entity2Id);
            });
        }

        [Fact]
        public void SaveChangesAsync_AddTwiceWithSameIdsThrows()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            var entity2 = new MyEntity(entityId, 5);

            Assert.Throws<ArgumentException>(() => sut.Add(entity2));
        }

        [Fact]
        public async Task AddThenGetReturnsTheSame()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);
            var entity2 = await sut.GetAsync<MyEntity>(entity.Id);

            Assert.Equal(entity, entity2);
        }

        [Fact]
        public async Task SaveChangesAsync_SavesNewAggregate()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId
                }
            };

            List<IUncommittedEventStoreRecord> eventsRecords = new List<IUncommittedEventStoreRecord>()
            {
                new UncommitedEventStoreRecord(entity.UncommittedEvents.ElementAt(0),
                    new Dictionary<string, string>()
                    {
                        { "TestKey", "TestValue" },
                        { BasicEventMetadataNames.AggregateClassId, entityClassId.ToString() }
                    })
            };

            IEnumerable<IUncommittedEventStoreRecord> pushedEvents = null;
            eventStore.WhenForAnyArgs(x => x.PushEventsAsync(Guid.Empty, null, null))
                .Do(ci => pushedEvents = ci.ArgAt<IEnumerable<IUncommittedEventStoreRecord>>(1));

            await sut.SaveChangesAsync();

            eventStore.Received(1).AddStream(entityId);
            eventStore.Received(1).PushEventsAsync(entityId, Arg.Any<IEnumerable<IUncommittedEventStoreRecord>>(), 0);
            eventStore.Received(1).CommitChangesAsync();

            pushedEvents.ShouldAllBeEquivalentTo(eventsRecords);
        }

        [Fact]
        public async Task SaveChangesAsync_PushesEventsForPublishing()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            var event1 = new SetFooEvent()
            {
                AggregateId = entityId
            };

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                event1
            };
            
            await sut.SaveChangesAsync();

            publishEventBuffer.Received(1).PushEvent(Arg.Is<IEventMessage>(x => x.Event == event1));
        }

        [Fact]
        public async Task SaveChangesAsync_PushedEventsHaveMetadata()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            var event1 = new SetFooEvent()
            {
                AggregateId = entityId
            };

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                event1
            };

            List<IEventMessage> eventMessages = new List<IEventMessage>();

            publishEventBuffer.WhenForAnyArgs(x => x.PushEvent(null)).Do(ci =>
            {
                eventMessages.Add(ci.ArgAt<IEventMessage>(0));
            });

            await sut.SaveChangesAsync();

            eventMessages.Should().HaveCount(1);
            eventMessages[0].Metadata.Should().HaveCount(2);
            eventMessages[0].Metadata.Should().Contain(x => x.Key == "TestKey" && x.Value == "TestValue");
            eventMessages[0].Metadata.Should().Contain(x => x.Key == BasicEventMetadataNames.AggregateClassId && x.Value == entityClassId.ToString());
        }

        [Fact]
        public async Task SaveChangesAsync_CommitsAggregate()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId
                }
            };

            await sut.SaveChangesAsync();
            Assert.Equal(1, entity.Version);
        }

        [Fact]
        public async Task SaveChangesAsync_CommitsOnlyChangedAggregates()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            await sut.SaveChangesAsync();
            Assert.Equal(0, entity.Version);
        }

        [Fact]
        public async Task DefaultFilters_GetsInitialFilters()
        {
            sut = new EventSourcedAggregateRepository(eventStore, entityTypeManager, publishEventBuffer,
                new[] { repositoryFilter1 }, eventMessageFactory);
            Assert.True(sut.DefaultFilters.SequenceEqual(new IRepositoryFilter[] {repositoryFilter1}));
        }

        [Fact]
        public async Task GetAsync_FilterGetsCalled()
        {
            repositoryFilter1.FilterResult<IEventSourcedAggregateRoot>(null)
                .ReturnsForAnyArgs(ci => ci.ArgAt<IEventSourcedAggregateRoot>(0));

            sut = new EventSourcedAggregateRepository(eventStore, entityTypeManager, publishEventBuffer,
                new[] { repositoryFilter1 }, eventMessageFactory);

            await sut.GetAsync<MyEntity2>(entity2Id);

            repositoryFilter1.Received(1).FilterResult<IEventSourcedAggregateRoot>(Arg.Is<IEventSourcedAggregateRoot>(x => x.Id == entity2Id));
        }

        [Fact]
        public async Task GetAsync_FilterReplacesReturnValue()
        {
            var replacementEntity = new MyEntity2(Guid.NewGuid(), 5);
            repositoryFilter1.FilterResult<IEventSourcedAggregateRoot>(null)
                .ReturnsForAnyArgs(replacementEntity);

            sut = new EventSourcedAggregateRepository(eventStore, entityTypeManager, publishEventBuffer,
                new[] { repositoryFilter1 }, eventMessageFactory);

            Assert.Equal(replacementEntity, await sut.GetAsync<MyEntity2>(entity2Id));
        }

        [Fact]
        public async Task SaveChangesAsync_FiltersAdded()
        {
            sut = new EventSourcedAggregateRepository(eventStore, entityTypeManager, publishEventBuffer,
                new[] { repositoryFilter1 }, eventMessageFactory);

            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId
                }
            };

            await sut.SaveChangesAsync();

            repositoryFilter1.Received(1).FilterAdded(entity);
        }

        [Fact]
        public async Task SaveChangesAsync_FiltersModified()
        {
            sut = new EventSourcedAggregateRepository(eventStore, entityTypeManager, publishEventBuffer,
                new[] { repositoryFilter1 }, eventMessageFactory);

            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId
                }
            };

            entity.Version++;

            await sut.SaveChangesAsync();

            repositoryFilter1.Received(1).FilterModified(entity);
        }

        public class SetFooEvent : DomainAggregateEvent
        {
        }

        public class MyEntity : IEventSourcedAggregateRoot
        {
            public MyEntity(Guid id, int foo) : this(id)
            {
                if (foo != 5)
                {
                    throw new InvalidOperationException();
                }
            }

            protected MyEntity(Guid id)
            {
                Id = id;
            }

            public Guid Id { get; private set; }
            public bool IsDeleted { get; set; }

            public IReadOnlyCollection<DomainAggregateEvent> UncommittedEvents { get; set; } =
                new List<DomainAggregateEvent>();

            public bool IsChanged => UncommittedEvents.Any();
            public int Version { get; set; }

            internal List<DomainAggregateEvent> LoadedEvents;

            public void Commit()
            {
                UncommittedEvents = new List<DomainAggregateEvent>();
                Version++;
            }

            public virtual void LoadState(AggregateState state)
            {
                if (LoadedEvents != null)
                {
                    throw new InvalidOperationException();
                }

                Version = state.Version;
                LoadedEvents = state.Events.ToList();
            }
        }

        public class MyEntity2 : MyEntity
        {
            public MyEntity2(Guid id, int foo) : base(id, foo)
            {
            }

            protected MyEntity2(Guid id) : base(id)
            {
            }
        }

        public class MyEntity3LoadsAsDeleted : MyEntity
        {
            public MyEntity3LoadsAsDeleted(Guid id) : base(id)
            {
            }

            public override void LoadState(AggregateState state)
            {
                base.LoadState(state);
                IsDeleted = true;
            }
        }
    }
}
