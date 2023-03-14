using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Testing.Infrastructure;
using NSubstitute;
using Xunit;

namespace Revo.Core.Tests.Events
{
    public class PublishEventBufferTests
    {
        private readonly PublishEventBuffer sut;
        private readonly IEventBus eventBus;

        public PublishEventBufferTests()
        {
            eventBus = Substitute.For<IEventBus>();
            sut = new PublishEventBuffer(eventBus);
        }

        [Fact]
        public void PushEvent_AddsToEvents()
        {
            var ev1 = new Event1().ToMessageDraft();
            sut.PushEvent(ev1);

            sut.Events.Should().HaveCount(1);
            sut.Events.Should().Contain(ev1);
        }

        [Fact]
        public async Task FlushAsync_PublishesEvents()
        {
            var ev1 = new Event1().ToMessageDraft();
            var ev2 = new Event1().ToMessageDraft();
            sut.PushEvent(ev1);
            sut.PushEvent(ev2);

            await sut.FlushAsync(CancellationToken.None);

            eventBus.Received(1).PublishAsync(ev1);
            eventBus.Received(1).PublishAsync(ev2);

            sut.Events.Should().HaveCount(0);
        }
        
        public class Event1 : IEvent
        {
        }
    }
}
