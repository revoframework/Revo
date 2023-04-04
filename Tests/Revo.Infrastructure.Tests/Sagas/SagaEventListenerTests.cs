using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Commands;
using Revo.Core.Events;
using Revo.Core.Services;
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

        public SagaEventListenerTests()
        {
            sagaEventDispatcher = Substitute.For<ISagaEventDispatcher>();

            var sagaEventDispatcherFactory = Substitute.For<IServiceFactory<ISagaEventDispatcher>>();
            sagaEventDispatcherFactory.GetRequiredService().Returns(sagaEventDispatcher);
            sut = new SagaEventListener(new SagaEventListener.SagaEventSequencer(), sagaEventDispatcherFactory,
                new CommandContextStack(), Substitute.For<IUnitOfWorkFactory>());
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

        public class Event1 : DomainEvent
        {
        }
    }
}
