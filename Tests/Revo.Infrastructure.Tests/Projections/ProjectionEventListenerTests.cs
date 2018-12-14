using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Core;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Infrastructure.Events.Async;
using Revo.Infrastructure.EventSourcing;
using Revo.Infrastructure.Projections;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Domain.Entities;
using Revo.Domain.Entities.EventSourcing;
using Revo.Domain.Events;
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
        private readonly IUnitOfWork unitOfWork;
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

            unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWork = Substitute.For<IUnitOfWork>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(unitOfWork);

            commandContextStack = new CommandContextStack();

            sut = Substitute.ForPartsOf<ProjectionEventListener>(projectionSubSystem, unitOfWorkFactory, commandContextStack);
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

            unitOfWork.Received(1).CommitAsync();
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
            
            await sut.HandleAsync(ev1, "MyProjectionEventListener");
            await sut.HandleAsync(ev2, "MyProjectionEventListener");
            await sut.HandleAsync(ev3, "MyProjectionEventListener");

            await sut.OnFinishedEventQueueAsync("MyProjectionEventListener");

            projectionSubSystem.WhenForAnyArgs(x => x.ExecuteProjectionsAsync(null, null, null))
                .Do(ci =>
                {
                    var events = ci.ArgAt<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(0);
                    events.Should().BeEquivalentTo(ev1, ev2, ev3);

                    commandContextStack.UnitOfWork.Should().Be(unitOfWork);
                });

            projectionSubSystem.Received(1).ExecuteProjectionsAsync(
                Arg.Any<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(),
                unitOfWork, Arg.Any<EventProjectionOptions>());
        }

        public class MyEvent : DomainAggregateEvent
        {
        }
    }
}
