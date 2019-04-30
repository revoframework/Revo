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
        
        [Fact]
        public async Task ProjectEventsAsync_CreatesTargetOnAggregateVersion0()
        {
            foreach (var message in eventMessages)
            {
                message.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
                message.SetMetadata(BasicEventMetadataNames.AggregateVersion, 0.ToString());
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

        [Fact]
        public async Task ProjectEventsAsync_FindTargetOnFollowingEvent()
        {
            sut = Substitute.ForPartsOf<TestEntityEventToPocoProjector>();
            sut.WhenForAnyArgs(x => x.Apply(null, Guid.Empty, null))
                .Do(ci =>
                {
                    sut.Target.Should().NotBeNull();
                    sut.Target.Should().Be(sut.LastFoundTarget);
                    sut.Target.Should().Be(ci.ArgAt<TestReadModel>(2));
                });

            eventMessages.Select((x, i) => (x, i)).ForEach(p => p.Item1.SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, (p.Item2 + 2).ToString())); //renumber
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

        [Fact]
        public async Task ProjectEventsAsync_IdempotentSkipsEventsWithLowerNumber()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.DidNotReceiveWithAnyArgs().Apply((IEventMessage<TestEvent1>) null);
            sut2.ReceivedWithAnyArgs(1).Apply((IEventMessage<TestEvent2>) null);
        }

        [Fact]
        public async Task ProjectEventsAsync_DoesNotSkipEventsWithoutNumber()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            eventMessages[1].SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.DidNotReceive().Apply((IEventMessage<TestEvent1>)eventMessages[0]);
            sut2.ReceivedWithAnyArgs(1).Apply((IEventMessage<TestEvent2>) eventMessages[1]);
        }

        [Fact]
        public async Task ProjectEventsAsync_UpdatesVersionFromEventNumber()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            eventMessages.Last().SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, 10.ToString());
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.Version.Should().Be(10);
        }

        [Fact]
        public async Task ProjectEventsAsync_UpdatesVersionFromEventCount()
        {
            var sut2 = Substitute.ForPartsOf<TestEntityEventToPocoProjectorVersioning>();
            sut2.CreatedTarget = new TestReadModelVersioned()
            {
                Version = 1
            };

            eventMessages[1].SetMetadata(BasicEventMetadataNames.StreamSequenceNumber, null);
            await sut2.ProjectEventsAsync(aggregateId, eventMessages);

            sut2.CreatedTarget.Version.Should().Be(2);
        }

        public class TestReadModel : ReadModelBase
        {
        }

        public class TestReadModelVersioned : ReadModelBase, IManuallyRowVersioned
        {
            public int Version { get; set; }
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

        public class TestEntityEventToPocoProjectorVersioning : EntityEventToPocoProjector<TestReadModelVersioned>
        {
            public TestReadModelVersioned CreatedTarget { get; set; }
            public TestReadModelVersioned FoundTarget { get; set; }

            public override async Task CommitChangesAsync()
            {
            }

            protected override async Task<TestReadModelVersioned> CreateProjectionTargetAsync(Guid aggregateId, IReadOnlyCollection<IEventMessage<DomainAggregateEvent>> events)
            {
                return CreatedTarget;
            }

            protected override async Task<TestReadModelVersioned> GetProjectionTargetAsync(Guid aggregateId)
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
