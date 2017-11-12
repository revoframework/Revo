using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core.Core;
using GTRevo.Core.Events;
using GTRevo.DataAccess.Entities;
using GTRevo.Infrastructure.Core.Domain;
using GTRevo.Infrastructure.Core.Domain.Events;
using GTRevo.Infrastructure.Core.Domain.EventSourcing;
using GTRevo.Infrastructure.EventSourcing;
using GTRevo.Testing.Core;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.EventSourcing
{
    public class EventSourcedRepositoryTests
    {
        private readonly IEventQueue eventQueue;
        private readonly IEventStore eventStore;
        private readonly IActorContext actorContext;
        private readonly IEntityTypeManager entityTypeManager;
        private readonly IRepositoryFilter repositoryFilter1;
        private readonly IRepositoryFilter repositoryFilter2;

        private Guid entityId = Guid.NewGuid();
        private Guid entity2Id = Guid.NewGuid();
        private Guid entityClassId = Guid.NewGuid();
        private Guid entity2ClassId = Guid.NewGuid();
        private Guid entity3ClassId = Guid.NewGuid();

        private EventSourcedAggregateRepository sut;

        public EventSourcedRepositoryTests()
        {
            eventQueue = Substitute.For<IEventQueue>();
            eventStore = Substitute.For<IEventStore>();
            actorContext = Substitute.For<IActorContext>();
            entityTypeManager = Substitute.For<IEntityTypeManager>();
            repositoryFilter1 = Substitute.For<IRepositoryFilter>();
            repositoryFilter2 = Substitute.For<IRepositoryFilter>();

            actorContext = Substitute.For<IActorContext>();
            actorContext.CurrentActorName.Returns("actor");
            FakeClock.Setup();
            
            eventStore.GetLastStateAsync(entity2Id)
                .Returns(new AggregateState(1, new List<DomainAggregateEvent>()
                {
                  new SetFooEvent()
                  {
                      AggregateId = entity2Id,
                      AggregateClassId = entity2ClassId
                  }
                }, false));

            entityTypeManager.GetClrTypeByClassId(entityClassId)
                .Returns(typeof(MyEntity));
            entityTypeManager.GetClrTypeByClassId(entity2ClassId)
                .Returns(typeof(MyEntity2));
            entityTypeManager.GetClrTypeByClassId(entity3ClassId)
                .Returns(typeof(MyEntity3LoadsAsDeleted));

            entityTypeManager.GetClassIdByClrType(typeof(MyEntity))
                .Returns(entityClassId);
            entityTypeManager.GetClassIdByClrType(typeof(MyEntity2))
                .Returns(entity2ClassId);
            entityTypeManager.GetClassIdByClrType(typeof(MyEntity3LoadsAsDeleted))
                .Returns(entity3ClassId);

            sut = new EventSourcedAggregateRepository(eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] {});
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
            Guid entityId = Guid.NewGuid();

            eventStore.GetLastStateAsync(entityId)
                .Returns(new AggregateState(1, new List<DomainAggregateEvent>()
                {
                    new SetFooEvent()
                    {
                        AggregateId = entityId,
                        AggregateClassId = entity3ClassId
                    }
                }, false));

            await Assert.ThrowsAsync<EntityDeletedException>(async () =>
            {
                await sut.GetAsync<MyEntity3LoadsAsDeleted>(entityId);
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
        public async Task SaveChangesAsync_SavesChangedAggregates()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId,
                    AggregateClassId = entityClassId
                }
            };

            List<DomainAggregateEventRecord> eventsRecords = new List<DomainAggregateEventRecord>()
            {
                new DomainAggregateEventRecord()
                {
                    ActorName = "actor",
                    AggregateVersion = 1,
                    DatePublished = FakeClock.Now,
                    Event = entity.UncommittedEvents.ElementAt(0)
                }
            };

            await sut.SaveChangesAsync();

            eventStore.Received(1).AddAggregate(entityId, entityClassId);
            eventStore.Received(1)
                .PushEventsAsync(entityId,
                    Arg.Is<IEnumerable<DomainAggregateEventRecord>>(x => x.Count() == eventsRecords.Count
                        && x.Select((y, i) => new {Y = y, I = i}).All(z => eventsRecords[z.I].Equals(z.Y))),
                    1);
            eventStore.Received(1).CommitChangesAsync();
        }

        [Fact]
        public async Task SaveChangesAsync_PushesEvents()
        {
            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            var event1 = new SetFooEvent()
            {
                AggregateId = entityId,
                AggregateClassId = entityClassId
            };

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                event1
            };
            
            await sut.SaveChangesAsync();

            eventQueue.Received(1).PushEvent(event1);
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
                    AggregateId = entityId,
                    AggregateClassId = entityClassId
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
            sut = new EventSourcedAggregateRepository(eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] { repositoryFilter1 });
            Assert.True(sut.DefaultFilters.SequenceEqual(new IRepositoryFilter[] {repositoryFilter1}));
        }

        [Fact]
        public async Task GetAsync_FilterGetsCalled()
        {
            repositoryFilter1.FilterResult<IEventSourcedAggregateRoot>(null)
                .ReturnsForAnyArgs(ci => ci.ArgAt<IEventSourcedAggregateRoot>(0));

            sut = new EventSourcedAggregateRepository(eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] { repositoryFilter1 });

            await sut.GetAsync<MyEntity2>(entity2Id);

            repositoryFilter1.Received(1).FilterResult<IEventSourcedAggregateRoot>(Arg.Is<IEventSourcedAggregateRoot>(x => x.Id == entity2Id));
        }

        [Fact]
        public async Task GetAsync_FilterReplacesReturnValue()
        {
            var replacementEntity = new MyEntity2(Guid.NewGuid(), 5);
            repositoryFilter1.FilterResult<IEventSourcedAggregateRoot>(null)
                .ReturnsForAnyArgs(replacementEntity);

            sut = new EventSourcedAggregateRepository(eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] { repositoryFilter1 });

            Assert.Equal(replacementEntity, await sut.GetAsync<MyEntity2>(entity2Id));
        }

        [Fact]
        public async Task SaveChangesAsync_FiltersAdded()
        {
            sut = new EventSourcedAggregateRepository(eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] { repositoryFilter1 });

            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId,
                    AggregateClassId = entityClassId
                }
            };

            await sut.SaveChangesAsync();

            repositoryFilter1.Received(1).FilterAdded(entity);
        }

        [Fact]
        public async Task SaveChangesAsync_FiltersModified()
        {
            sut = new EventSourcedAggregateRepository(eventStore, actorContext,
                entityTypeManager, eventQueue, new IRepositoryFilter[] { repositoryFilter1 });

            var entity = new MyEntity(entityId, 5);
            sut.Add(entity);

            entity.UncommittedEvents = new List<DomainAggregateEvent>()
            {
                new SetFooEvent()
                {
                    AggregateId = entityId,
                    AggregateClassId = entityClassId
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

            public IEnumerable<DomainAggregateEvent> UncommittedEvents { get; set; } =
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
