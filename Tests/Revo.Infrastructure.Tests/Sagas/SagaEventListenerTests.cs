using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Transactions;
using Revo.Domain.Events;
using Revo.Infrastructure.Sagas;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Sagas
{
    public class SagaEventListenerTests
    {
        private readonly SagaEventListener sut;
        private readonly ISagaEventDispatcher sagaEventDispatcher;
        private readonly IUnitOfWork unitOfWork;
        private readonly CommandContextStack commandContextStack;
        private readonly IServiceScope scope;

        public SagaEventListenerTests()
        {
            sagaEventDispatcher = Substitute.For<ISagaEventDispatcher>();
            unitOfWork = Substitute.For<IUnitOfWork>();
            var unitOfWorkFactory = Substitute.For<IUnitOfWorkFactory>();
            unitOfWorkFactory.CreateUnitOfWork().Returns(unitOfWork);
            commandContextStack = new CommandContextStack();

            var scopeServiceProvider = Substitute.For<IServiceProvider>();
            scopeServiceProvider.GetService(typeof(ISagaEventDispatcher)).Returns(sagaEventDispatcher);
            scopeServiceProvider.GetService(typeof(IUnitOfWorkFactory)).Returns(unitOfWorkFactory);
            scopeServiceProvider.GetService(typeof(CommandContextStack)).Returns(commandContextStack);

            scope = Substitute.For<IServiceScope>();
            scope.ServiceProvider.Returns(scopeServiceProvider);

            var serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            serviceScopeFactory.CreateScope().Returns(scope);

            sut = new SagaEventListener(new SagaEventListener.SagaEventSequencer(), serviceScopeFactory);
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_DispatchesEvents()
        {
            var event1 = new Event1().ToMessageDraft();

            List<IEventMessage<DomainEvent>> events = null;
            sagaEventDispatcher.WhenForAnyArgs(x => x.DispatchEventsToSagas(null))
                .Do(ci =>
                {
                    events = ci.ArgAt<IEnumerable<IEventMessage<DomainEvent>>>(0).ToList();
                });
            
            await sut.HandleAsync(event1, "SagaEventListener");
            await sut.OnFinishedEventQueueAsync("SagaEventListener");

            sagaEventDispatcher.ReceivedWithAnyArgs(1).DispatchEventsToSagas(null);

            events.Should().NotBeNull();
            events.Should().BeEquivalentTo(new[]
            {
                event1
            }, options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task OnFinishedEventQueueAsync_DispatchesInOwnUnitOfWorkScope()
        {
            sagaEventDispatcher.WhenForAnyArgs(x => x.DispatchEventsToSagas(null))
                .Do(ci =>
                {
                    commandContextStack.UnitOfWork.Should().BeSameAs(unitOfWork);
                    scope.DidNotReceive().Dispose();
                });

            await sut.HandleAsync(new Event1().ToMessageDraft(), "SagaEventListener");
            await sut.OnFinishedEventQueueAsync("SagaEventListener");

            Received.InOrder(() =>
            {
                unitOfWork.Begin();
                sagaEventDispatcher.DispatchEventsToSagas(Arg.Any<IEnumerable<IEventMessage<DomainEvent>>>());
                unitOfWork.CommitAsync();
                scope.Dispose();
            });
            commandContextStack.PeekOrDefault.Should().BeNull();
        }

        public class Event1 : DomainEvent
        {
        }
    }
}
