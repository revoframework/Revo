using FluentAssertions;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Tenancy;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.Projections;
using Revo.Infrastructure.Tenancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Revo.Infrastructure.Tests.Projections
{
    public class ProjectionEventListenerTests
    {
        private readonly ProjectionEventListener sut;
        private readonly IAsyncEventSequencer<DomainAggregateEvent> sequencer;
        private readonly IEntityEventProjector myEntity1Projector;
        private readonly IEntityEventProjector myEntity2Projector;
        private readonly IProjectionSubSystem projectionSubSystem;
        private readonly IUnitOfWorkFactory unitOfWorkFactory;
        private readonly List<IUnitOfWork> unitsOfWork = new();
        private readonly ITenantProvider tenantProvider;
        private readonly CommandContextStack commandContextStack;

        private readonly Guid aggregate1Id = Guid.Parse("F2280E17-8FC0-4F49-B57B-4335AEFC0063");
        private readonly Guid aggregate2Id = Guid.Parse("8D165937-CB74-4052-B9E3-A7E56AA49106");

        public ProjectionEventListenerTests()
        {
            myEntity1Projector = Substitute.For<IEntityEventProjector>();

            myEntity2Projector = Substitute.For<IEntityEventProjector>();

            sequencer = Substitute.For<IAsyncEventSequencer<DomainAggregateEvent>>();
            sequencer.GetEventSequencing(null).ReturnsForAnyArgs(new[]
            {
                new EventSequencing()
                {
                    SequenceName = "MyProjectionEventListener",
                    EventSequenceNumber = 1
                }
            });
            sequencer.ShouldAttemptSynchronousDispatch(null).ReturnsForAnyArgs(true);

            projectionSubSystem = Substitute.For<IProjectionSubSystem>();
            Func<IProjectionSubSystem> projectionSubSystemFunc = () => projectionSubSystem;

            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(ci =>
            {
                unitsOfWork.Add(Substitute.For<IUnitOfWork>());
                return unitsOfWork.Last();
            });

            commandContextStack = new CommandContextStack();
            Func<CommandContextStack> commandContextStackFunc = () => commandContextStack;

            tenantProvider = Substitute.For<ITenantProvider>();

            sut = Substitute.ForPartsOf<ProjectionEventListener>(projectionSubSystemFunc, unitOfWorkFactory,
                commandContextStackFunc, tenantProvider);
            sut.EventSequencer.Returns(sequencer);
        }

        [Fact]
        public async Task OnFinishedEventQueue_CommitsUow()
        {
            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>());

            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");

            unitsOfWork.Should().HaveCount(1);
            unitsOfWork.Last().Received(1).CommitAsync();
        }

        [Fact]
        public async Task OnFinishedEventQueue_PopsCommandStack()
        {
            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>());

            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");

            commandContextStack.PeekOrDefault.Should().BeNull();
        }

        [Fact]
        public async Task OnFinishedEventQueue_ExecutesProjections()
        {
            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>());

            var ev2 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate2Id
            }, new Dictionary<string, string>());

            var ev3 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>());

            projectionSubSystem.WhenForAnyArgs(x => x.ExecuteProjectionsAsync(null, null, null))
                .Do(ci =>
                {
                    unitsOfWork.Should().HaveCount(1);
                    commandContextStack.UnitOfWork.Should().Be(unitsOfWork[0]);
                    unitsOfWork[0].Should().Be(ci.Arg<IUnitOfWork>());
                });

            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.HandleAsync(ev2, "MyProjectionEventListener");
            await sut.HandleAsync(ev3, "MyProjectionEventListener");

            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");

            projectionSubSystem.Received(1).ExecuteProjectionsAsync(
                Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(events => events.SequenceEqual(new[] { ev1, ev2, ev3 })),
                unitsOfWork[0], Arg.Any<EventProjectionOptions>());
        }

        [Fact]
        public async Task OnFinishedEventQueue_GroupByTenants()
        {
            var tenant1 = Substitute.For<ITenant>();
            tenant1.Id.Returns(Guid.Parse("4FEEE256-9949-45B8-9F04-ED73387B9AF4"));
            tenantProvider.GetTenant(tenant1.Id).Returns(tenant1);
            var tenant2 = Substitute.For<ITenant>();
            tenant2.Id.Returns(Guid.Parse("D3B555D1-622B-407B-B742-957ED55BBB7C"));
            tenantProvider.GetTenant(tenant2.Id).Returns(tenant2);

            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateTenantId, tenant1.Id.ToString() }
            });

            var ev2 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate2Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateTenantId, tenant2.Id.ToString() }
            });

            projectionSubSystem.WhenForAnyArgs(x => x.ExecuteProjectionsAsync(null, null, null))
                .Do(ci =>
                {
                    unitsOfWork.Should().HaveCountGreaterOrEqualTo(1);
                    commandContextStack.UnitOfWork.Should().Be(unitsOfWork.Last());
                    unitsOfWork.Last().Should().Be(ci.Arg<IUnitOfWork>());
                });

            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.HandleAsync(ev2, "MyProjectionEventListener");
            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");

            unitsOfWork.Should().HaveCount(2);

            projectionSubSystem.Received(1).ExecuteProjectionsAsync(
                Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(events => events.SequenceEqual(new[] { ev1 })),
                unitsOfWork[0], Arg.Any<EventProjectionOptions>());

            projectionSubSystem.Received(1).ExecuteProjectionsAsync(
                Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(events => events.SequenceEqual(new[] { ev2 })),
                unitsOfWork[1], Arg.Any<EventProjectionOptions>());
        }

        [Fact]
        public async Task OnFinishedEventQueue_UoWCreatedInTenantContext()
        {
            var tenant1 = Substitute.For<ITenant>();
            tenant1.Id.Returns(Guid.Parse("4FEEE256-9949-45B8-9F04-ED73387B9AF4"));
            tenantProvider.GetTenant(tenant1.Id).Returns(tenant1);

            var ev1 = new EventMessage<DomainAggregateEvent>(new MyEvent()
            {
                AggregateId = aggregate1Id
            }, new Dictionary<string, string>()
            {
                { BasicEventMetadataNames.AggregateTenantId, tenant1.Id.ToString() }
            });

            unitOfWorkFactory.WhenForAnyArgs(x => x.CreateUnitOfWork())
                .Do(ci =>
                {
                    TenantContextOverride.Current?.Tenant.Should().Be(tenant1);
                });

            projectionSubSystem.WhenForAnyArgs(x => x.ExecuteProjectionsAsync(null, null, null))
                .Do(ci =>
                {
                    TenantContextOverride.Current?.Tenant.Should().Be(tenant1);
                });

            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");

            projectionSubSystem.Received(1).ExecuteProjectionsAsync(
                Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(events => events.SequenceEqual(new[] { ev1 })),
                unitsOfWork[0], Arg.Any<EventProjectionOptions>());
            
            TenantContextOverride.Current?.Tenant.Should().Be(null);
        }

        public class MyEvent : DomainAggregateEvent
        {
        }
    }
}
