using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using Revo.Core.Events;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Upgrades;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.EventStores;
using Revo.Infrastructure.Repositories;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.Infrastructure.Tests.Repositories
{
    public class EventSourcedAggregateFactoryTests
    {

        private readonly IEventStreamUpgrader eventStreamUpgrader;
        private readonly IEntityTypeManager entityTypeManager;
        private EventSourcedAggregateFactory sut;
        private Func<IEnumerable<IEventMessage<DomainAggregateEvent>>, IReadOnlyDictionary<string, string>,
            IEnumerable<IEventMessage<DomainAggregateEvent>>> eventStreamUpgradeFunc = (stream, _) => stream;

        private readonly Guid entity1ClassId = Guid.Parse("C247423D-6228-4D6F-84A4-601718B8770B");
        private readonly Guid entity2ClassId = Guid.Parse("7AFB581A-47F5-42B0-ABE2-7C789E4E6C55");

        public EventSourcedAggregateFactoryTests()
        {

            eventStreamUpgrader = Substitute.For<IEventStreamUpgrader>();
            eventStreamUpgrader.UpgradeStream(null, null).ReturnsForAnyArgs(ci =>
            {
                var stream = ci.Arg<IEnumerable<IEventMessage<DomainAggregateEvent>>>();
                var aggregateMetadata = ci.Arg<IReadOnlyDictionary<string, string>>();
                return eventStreamUpgradeFunc(stream, aggregateMetadata);
            });

            DomainClassInfo[] domainClasses = new[]
            {
                new DomainClassInfo(entity1ClassId, null, typeof(MyEntity)),
                new DomainClassInfo(entity2ClassId, null, typeof(MyEntity2))
            };

            entityTypeManager = Substitute.For<IEntityTypeManager>();
            entityTypeManager.GetClassInfoByClassId(Guid.Empty)
                .ReturnsForAnyArgs(ci => domainClasses.Single(x => x.Id == ci.Arg<Guid>()));
            entityTypeManager.TryGetClassInfoByClrType(null)
                .ReturnsForAnyArgs(ci => domainClasses.SingleOrDefault(x => x.ClrType == ci.Arg<Type>()));
            entityTypeManager.GetClassInfoByClrType(null)
                .ReturnsForAnyArgs(ci => domainClasses.Single(x => x.ClrType == ci.Arg<Type>()));

            sut = new EventSourcedAggregateFactory(eventStreamUpgrader, new EntityFactory(), entityTypeManager);
        }

        [Fact]
        public void ConstructAndLoadEntityFromEvents()
        {
            Guid entityId = Guid.Parse("571BCB87-49C7-44AE-A96F-CEBA645A8E6D");
            var eventStoreRecords = new List<IEventStoreRecord>()
            {
                new FakeEventStoreRecord()
                {
                    Event = new SetFooEvent()
                    {
                        AggregateId = entityId
                    },
                    StreamSequenceNumber = 1
                }
            };
            var result = sut.ConstructAndLoadEntityFromEvents(entityId,
                new Dictionary<string, string>()
                {
                    {AggregateEventStreamMetadataNames.ClassId, entity2ClassId.ToString()}
                },
                eventStoreRecords);

            result.Id.Should().Be(entityId);
            result.Should().BeOfType<MyEntity2>();
            var typedResult = (MyEntity2) result;
            typedResult.LoadedEvents.Should().Equal(eventStoreRecords.Select(x => (DomainAggregateEvent) x.Event));
        }
        
        [Fact]
        public void ConstructAndLoadEntityFromEvents_LoadsAggregateVersionFromSequenceNumber()
        {
            Guid entityId = Guid.Parse("571BCB87-49C7-44AE-A96F-CEBA645A8E6D");
            var eventStoreRecords = new List<IEventStoreRecord>()
            {
                new FakeEventStoreRecord()
                {
                    Event = new SetFooEvent()
                    {
                        AggregateId = entityId
                    },
                    StreamSequenceNumber = 1
                }
            };
            var result = sut.ConstructAndLoadEntityFromEvents(entityId,
                new Dictionary<string, string>()
                {
                    {AggregateEventStreamMetadataNames.ClassId, entity2ClassId.ToString()}
                },
                eventStoreRecords);

            result.Should().BeOfType<MyEntity2>();
            result.Version.Should().Be(1);
        }

        [Fact]
        public void ConstructAndLoadEntityFromEvents_LoadsAggregateVersionFromAVEventMetadata()
        {
            Guid entityId = Guid.Parse("571BCB87-49C7-44AE-A96F-CEBA645A8E6D");
            var eventStoreRecords = new List<IEventStoreRecord>()
            {
                new FakeEventStoreRecord()
                {
                    Event = new SetFooEvent()
                    {
                        AggregateId = entityId
                    },
                    StreamSequenceNumber = 1,
                    AdditionalMetadata = new Dictionary<string, string>()
                    {
                        {BasicEventMetadataNames.AggregateVersion, "2" }
                    }
                }
            };
            var result = sut.ConstructAndLoadEntityFromEvents(entityId,
                new Dictionary<string, string>()
                {
                    {AggregateEventStreamMetadataNames.ClassId, entity2ClassId.ToString()}
                },
                eventStoreRecords);

            result.Should().BeOfType<MyEntity2>();
            result.Version.Should().Be(2);
        }

        [Fact]
        public void ConstructAndLoadEntityFromEvents_UpgradesEvents()
        {
            Guid entityId = Guid.Parse("571BCB87-49C7-44AE-A96F-CEBA645A8E6D");
            var eventStoreRecords = new List<IEventStoreRecord>()
            {
                new FakeEventStoreRecord()
                {
                    Event = new SetFooEvent()
                    {
                        AggregateId = entityId
                    },
                    StreamSequenceNumber = 1
                }
            };

            var upgradedMessages = new[]
            {
                (IEventMessage<DomainAggregateEvent>) EventMessage.FromEvent(new SetFooEventV2()
                    {
                        AggregateId = entityId
                    },
                    new Dictionary<string, string>()
                    {
                        { BasicEventMetadataNames.StreamSequenceNumber, "1" }
                    }),
                (IEventMessage<DomainAggregateEvent>) EventMessage.FromEvent(new SetBarEvent()
                    {
                        AggregateId = entityId
                    },
                    new Dictionary<string, string>()
                    {
                        { BasicEventMetadataNames.StreamSequenceNumber, "2" }
                    })
            };

            eventStreamUpgradeFunc = (events, metadata) =>
            {
                if (events.Count() == eventStoreRecords.Count
                    && events.Select((x, i) => new { x, i }).All(x => x.x.Event == eventStoreRecords[x.i].Event))
                {
                    return upgradedMessages;
                }

                return events;
            };

            var result = sut.ConstructAndLoadEntityFromEvents(entityId,
                new Dictionary<string, string>()
                {
                    {AggregateEventStreamMetadataNames.ClassId, entity2ClassId.ToString()}
                },
                eventStoreRecords);

            result.Should().BeOfType<MyEntity2>();
            var typedResult = (MyEntity2) result;
            typedResult.LoadedEvents.Should().Equal(upgradedMessages.Select(x => x.Event));
            typedResult.Version.Should().Be(1);
        }
        
        public class SetFooEvent : DomainAggregateEvent
        {
        }

        public class SetBarEvent : DomainAggregateEvent
        {
        }

        public class SetFooEventV2 : DomainAggregateEvent
        {
        }

        public class MyEntity : IEventSourcedAggregateRoot
        {
            public MyEntity(Guid id, int foo) : this(id)
            {
                throw new InvalidOperationException();
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
    }
}