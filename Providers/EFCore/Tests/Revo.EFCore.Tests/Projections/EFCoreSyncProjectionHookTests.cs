using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.EFCore.Projections;
using Revo.Testing.Infrastructure;
using Xunit;

namespace Revo.EFCore.Tests.Projections;

public class EFCoreSyncProjectionHookTests
{
    private EFCoreSyncProjectionHook sut;
    private ICommandContext commandContext = Substitute.For<ICommandContext>();
    private IEFCoreProjectionSubSystem projectionSubSystem = Substitute.For<IEFCoreProjectionSubSystem>();
    private IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private IPublishEventBuffer eventBuffer = Substitute.For<IPublishEventBuffer>();
    private List<IEventMessage> events = new();

    public EFCoreSyncProjectionHookTests()
    {
        unitOfWork.EventBuffer.Returns(eventBuffer);
        eventBuffer.Events.Returns(events);

        sut = new EFCoreSyncProjectionHook(commandContext, projectionSubSystem);
    }

    [Fact]
    public async Task ProjectsBeforeCommit()
    {
        events.Add(new TestEvent().ToMessageDraft());
        commandContext.UnitOfWork.Returns(unitOfWork);

        await sut.OnBeforeCommitAsync();

        projectionSubSystem.Received(1).ExecuteProjectionsAsync(
            Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(
                evs =>  evs.SequenceEqual(events)),
            unitOfWork,
            Arg.Is<EFCoreEventProjectionOptions>(x => x.IsSynchronousProjection));
    }

    [Fact]
    public async Task ProjectsOnlyAdditionalEvents()
    {
        events.Add(new TestEvent().ToMessageDraft());
        commandContext.UnitOfWork.Returns(unitOfWork);

        await sut.OnBeforeCommitAsync();
        events.Add(new TestEvent().ToMessageDraft());
        await sut.OnBeforeCommitAsync();

        projectionSubSystem.ReceivedWithAnyArgs(2).ExecuteProjectionsAsync(null, null, null);

        projectionSubSystem.Received(1).ExecuteProjectionsAsync(
            Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(
                evs => evs.SequenceEqual(new[] { events[0] })),
            unitOfWork,
            Arg.Is<EFCoreEventProjectionOptions>(x => x.IsSynchronousProjection));

        projectionSubSystem.Received(1).ExecuteProjectionsAsync(
            Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(
                evs => evs.SequenceEqual(new[] { events[1] })),
            unitOfWork,
            Arg.Is<EFCoreEventProjectionOptions>(x => x.IsSynchronousProjection));
    }

    [Fact]
    public async Task ProjectsAgainAfterCommitSucceeded()
    {
        events.Add(new TestEvent().ToMessageDraft());
        commandContext.UnitOfWork.Returns(unitOfWork);

        await sut.OnBeforeCommitAsync();
        await sut.OnCommitSucceededAsync();
        await sut.OnBeforeCommitAsync();

        projectionSubSystem.Received(2).ExecuteProjectionsAsync(
            Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(
                evs => evs.SequenceEqual(events)),
            unitOfWork,
            Arg.Is<EFCoreEventProjectionOptions>(x => x.IsSynchronousProjection));
    }

    [Fact]
    public async Task ProjectsAgainAfterCommitFailed()
    {
        events.Add(new TestEvent().ToMessageDraft());
        commandContext.UnitOfWork.Returns(unitOfWork);

        await sut.OnBeforeCommitAsync();
        await sut.OnCommitFailedAsync();
        await sut.OnBeforeCommitAsync();

        projectionSubSystem.Received(2).ExecuteProjectionsAsync(
            Arg.Is<IReadOnlyCollection<IEventMessage<DomainAggregateEvent>>>(
                evs => evs.SequenceEqual(events)),
            unitOfWork,
            Arg.Is<EFCoreEventProjectionOptions>(x => x.IsSynchronousProjection));
    }

    private class TestEvent : DomainAggregateEvent
    {
    }
}