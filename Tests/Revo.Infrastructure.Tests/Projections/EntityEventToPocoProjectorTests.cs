using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using MoreLinq;
using Revo.Core.Events;
using Revo.Domain.ReadModel;
using Revo.Infrastructure.Projections;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Revo.DataAccess.Entities;
using Revo.Domain.Events;
using Xunit;

namespace Revo.Infrastructure.Tests.Projections
{
    public class EntityEventToPocoProjectorTests
    {
        private TestEntityEventToPocoProjector sut;
        private Guid aggregateId = Guid.Parse("7275CF99-C345-4360-B2F6-8F40DBBE47E4");
        private List<IEventMessageDraft<DomainAggregateEvent>> eventMessages;

        public EntityEventToPocoProjectorTests()
        {
            eventMessages = new DomainAggregateEvent[]
            {
                new TestEvent1(),
                new TestEvent2()
            }.Select((x, i) =>
            {
                var message = x.ToMessageDraft();
                message.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (i + 1).ToString());
                message.SetMetadata(BasicEventMetadataNames.AggregateVersion, "1");
                return message;
            }).ToList();
        }

        [Fact]
        public async Task ProjectEventsAsync_CallsApplyWithTargetParam()
        {
            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjector>();
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.Received(1).Apply((IEventMessage<TestEvent1>) eventMessages[0], aggregateId, sut.LastCreatedTarget);
        }

        [Fact]
        public async Task ProjectEventsAsync_CreatesTargetOnFirstEvent()
        {
            eventMessages.ForEach(p => p.SetMetadata(BasicEventMetadataNames.AggregateVersion, null));

            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjector>();
            sut.WhenForAnyArgs(x => x.Apply(null, Guid.Empty, null))
                .Do(ci =>
                {
                    sut.Target.Should().NotBeNull();
                    sut.Target.Should().Be(sut.LastCreatedTarget);
                    sut.Target.Should().Be(ci.ArgAt<TestReadModel>(2));
                });

            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.LastCreatedTarget.Should().NotBeNull();
            sut.LastFoundTarget.Should().BeNull();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProjectEventsAsync_CreatesTargetOnAggregateVersion1(bool nullStreamSequenceNumbers)
        {
            foreach (var message in eventMessages)
            {
                if (nullStreamSequenceNumbers)
                {
                    message.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
                }

                message.SetMetadata(BasicEventMetadataNames.AggregateVersion, 1.ToString());
            }

            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjector>();
            sut.WhenForAnyArgs(x => x.Apply(null, Guid.Empty, null))
                .Do(ci =>
                {
                    sut.Target.Should().NotBeNull();
                    sut.Target.Should().Be(sut.LastCreatedTarget);
                    sut.Target.Should().Be(ci.ArgAt<TestReadModel>(2));
                });

            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.LastCreatedTarget.Should().NotBeNull();
            sut.LastFoundTarget.Should().BeNull();
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public async Task ProjectEventsAsync_FindTargetOnFollowingEvent(bool usingSequenceNumbers, bool usingAggregateVersions)
        {
            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjector>();
            sut.WhenForAnyArgs(x => x.Apply(null, Guid.Empty, null))
                .Do(ci =>
                {
                    sut.Target.Should().NotBeNull();
                    sut.Target.Should().Be(sut.LastFoundTarget);
                    sut.Target.Should().Be(ci.ArgAt<TestReadModel>(2));
                });


            if (usingSequenceNumbers)
            {
                eventMessages.Select((x, i) => (x, i)).ForEach(p =>
                    p.Item1.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber,
                        (p.Item2 + 2).ToString())); //renumber
                eventMessages.ForEach(x => x.SetMetadata(BasicEventMetadataNames.AggregateVersion, null));
            }

            if (usingAggregateVersions)
            {
                eventMessages.ForEach(x => x.SetMetadata(BasicEventMetadataNames.AggregateVersion, "2"));
            }

            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.LastFoundTarget.Should().NotBeNull();
            sut.LastCreatedTarget.Should().BeNull();
        }

        [Fact]
        public async Task ProjectEventsAsync_SkipsProjectionIfTargetNotCreated()
        {
            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjectorNullTarget>();
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.DidNotReceiveWithAnyArgs().Apply(null, Guid.Empty, null);
        }

        [Fact]
        public async Task ProjectEventsAsync_SkipsProjectionIfTargetNotFound()
        {
            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjectorNullTarget>();
            eventMessages.Select((x, i) => (x, i)).ForEach(p => p.Item1.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (p.Item2 + 2).ToString())); //renumber
            await sut.ProjectEventsAsync(aggregateId, eventMessages);

            sut.DidNotReceiveWithAnyArgs().Apply(null, Guid.Empty, null);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProjectEventsAsync_IdempotentSkipsEventsWithLowerNumber(bool usingSequenceNumbers)
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelVersioned>>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            if (usingSequenceNumbers)
            {
                eventMessages.ForEach(x => x.SetMetadata(BasicEventMetadataNames.AggregateVersion, null));
            }
            else
            {
                eventMessages[1].SetMetadata(BasicEventMetadataNames.AggregateVersion, "2");    
            }

            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.DidNotReceiveWithAnyArgs().Apply((IEventMessage<TestEvent1>) null);
            sut2.ReceivedWithAnyArgs(1).Apply((IEventMessage<TestEvent2>) null);
        }

        [Fact]
        public async Task ProjectEventsAsync_DoesNotSkipEventsWithoutNumber()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelVersioned>>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            eventMessages[1].SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
            eventMessages[1].SetMetadata(BasicEventMetadataNames.AggregateVersion, null);
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.DidNotReceive().Apply((IEventMessage<TestEvent1>)eventMessages[0]);
            sut2.ReceivedWithAnyArgs(1).Apply((IEventMessage<TestEvent2>) eventMessages[1]);
        }

        [Fact]
        public async Task ProjectEventsAsync_Versioned_UpdatesVersionFromEventNumber()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelVersioned>>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, 10.ToString());
            eventMessages.Last().SetMetadata(BasicEventMetadataNames.AggregateVersion, null);
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.Version.Should().Be(10);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProjectEventsAsync_Versioned_UpdatesVersionFromAggregateVersion(bool nullSequenceNumbers)
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelVersioned>>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 0
            };

            if (nullSequenceNumbers)
            {
                eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
            }

            eventMessages[0].SetMetadata(BasicEventMetadataNames.AggregateVersion, 1.ToString());
            eventMessages[1].SetMetadata(BasicEventMetadataNames.AggregateVersion, 2.ToString());

            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.Version.Should().Be(2);
        }

        [Fact]
        public async Task ProjectEventsAsync_EventNumbered_UpdatesEventNumber()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelEventNumbered>>();
            sut2.CreatedTarget = new TestReadModelEventNumbered()
            {
                EventNumber = 1
            };

            eventMessages.Last().SetMetadata(BasicEventMetadataNames.AggregateVersion, null);
            eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, 10.ToString());
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.EventNumber.Should().Be(10);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProjectEventsAsync_EventNumbered_UpdatesEventNumberFromAggregateVersion(bool nullSequenceNumbers)
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelEventNumbered>>();
            sut2.CreatedTarget = new TestReadModelEventNumbered()
            {
                EventNumber = 0
            };

            if (nullSequenceNumbers)
            {
                eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
            }

            eventMessages[0].SetMetadata(BasicEventMetadataNames.AggregateVersion, 1.ToString());
            eventMessages[1].SetMetadata(BasicEventMetadataNames.AggregateVersion, 2.ToString());
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.EventNumber.Should().Be(2);
        }

        [Fact]
        public async Task ProjectEventsAsync_VersionedAndEventNumbered_UpdatesBoth()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelVersionedEventNumbered>>();
            sut2.CreatedTarget = new TestReadModelVersionedEventNumbered()
            {
                EventNumber = 1,
                Version = 1
            };

            eventMessages.Last().SetMetadata(BasicEventMetadataNames.AggregateVersion, null);
            eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, 10.ToString());
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.EventNumber.Should().Be(10);
            sut2.CreatedTarget.Version.Should().Be(2);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ProjectEventsAsync_VersionedAndEventNumbered_UpdatesFromAggregateVersion(bool nullSequenceNumbers)
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning<TestReadModelVersionedEventNumbered>>();
            sut2.CreatedTarget = new TestReadModelVersionedEventNumbered()
            {
                EventNumber = 0,
                Version = 0
            };

            if (nullSequenceNumbers)
            {
                eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
            }

            eventMessages[0].SetMetadata(BasicEventMetadataNames.AggregateVersion, 1.ToString());
            eventMessages[1].SetMetadata(BasicEventMetadataNames.AggregateVersion, 2.ToString());
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.EventNumber.Should().Be(2);
            sut2.CreatedTarget.Version.Should().Be(1);
        }

        public class TestReadModel : ReadModelBase
        {
        }

        public class TestReadModelVersioned : ReadModelBase, IManuallyRowVersioned
        {
            public int Version { get; set; }
        }

        public class TestReadModelVersionedEventNumbered : TestReadModelVersioned, IEventNumberVersioned
        {
            public int EventNumber { get; set; }
        }

        public class TestReadModelEventNumbered : ReadModelBase, IEventNumberVersioned
        {
            public int EventNumber { get; set; }
        }

        public class TestEntityEventToPocoProjector : EntityEventToPocoProjector<TestReadModel>
        {
            public new TestReadModel Target => base.Target;
            public TestReadModel LastCreatedTarget { get; private set; }
            public TestReadModel LastFoundTarget { get; private set; }

            public override async Task CommitChangesAsync()
            {
            }

            protected override async Task<TestReadModel> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
            {
                return LastCreatedTarget = new TestReadModel();
            }

            protected override async Task<TestReadModel> GetProjectionTargetAsync(Guid aggregateId)
            {
                return LastFoundTarget = new TestReadModel();
            }

            public virtual void Apply(IEventMessage<TestEvent1> ev, Guid aggregateId, TestReadModel target)
            {
            }
        }

        public class TestEntityEventToPocoProjectorNullTarget : TestEntityEventToPocoProjector
        {
            protected override async Task<TestReadModel> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
            {
                return null;
            }

            protected override async Task<TestReadModel> GetProjectionTargetAsync(Guid aggregateId)
            {
                return null;
            }
        }

        public class TestEntityEventToPocoProjectorVersioning<T> : EntityEventToPocoProjector<T>
        {
            public T CreatedTarget { get; set; }
            public T FoundTarget { get; set; }

            public override async Task CommitChangesAsync()
            {
            }

            protected override async Task<T> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
            {
                return CreatedTarget;
            }

            protected override async Task<T> GetProjectionTargetAsync(Guid aggregateId)
            {
                return FoundTarget;
            }

            public virtual void Apply(IEventMessage<TestEvent1> ev)
            {
            }

            public virtual void Apply(IEventMessage<TestEvent2> ev)
            {
            }
        }

        public class TestEvent1 : DomainAggregateEvent
        {
        }

        public class TestEvent2 : DomainAggregateEvent
        {
        }
    }
}
