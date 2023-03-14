using System.Threading.Tasks;
using FluentAssertions;
using Revo.Core.Events;
using Revo.Infrastructure.Events;
using Revo.Infrastructure.Events.Metadata;
using NSubstitute;
using Xunit;

namespace Revo.Infrastructure.Tests.Events
{
    public class EventMessageFactoryTests
    {
        private EventMessageFactory sut;
        private IEventMetadataProvider[] eventMetadataProviders;

        public EventMessageFactoryTests()
        {
            eventMetadataProviders = new[]
            {
                Substitute.For<IEventMetadataProvider>(),
                Substitute.For<IEventMetadataProvider>()
            };

            sut = new EventMessageFactory(eventMetadataProviders);
        }

        [Fact]
        public async Task CreateMessageAsync_CreatesCorrectMessageType()
        {
            IEvent @event = new Event1();
            IEventMessageDraft message = await sut.CreateMessageAsync(@event);
            message.Should().BeOfType<EventMessageDraft<Event1>>();
            message.Event.Should().Be(@event);
        }

        [Fact]
        public async Task CreateMessageAsync_InjectsMetadata()
        {
            eventMetadataProviders[0].GetMetadataAsync(null).ReturnsForAnyArgs(new(string key, string value)[]
            {
                ("key1", "value1"),
                ("key2", "value2")
            });

            eventMetadataProviders[1].GetMetadataAsync(null).ReturnsForAnyArgs(new(string key, string value)[]
            {
                ("key3", "value3")
            });

            IEvent @event = new Event1();
            IEventMessageDraft message = await sut.CreateMessageAsync(@event);

            message.Metadata.Should().HaveCount(3);
            message.Metadata.Should().Contain(x => x.Key == "key1" && x.Value == "value1");
            message.Metadata.Should().Contain(x => x.Key == "key2" && x.Value == "value2");
            message.Metadata.Should().Contain(x => x.Key == "key3" && x.Value == "value3");
        }

        [Fact]
        public async Task CreateMessageAsync_MetadataOverrides()
        {
            eventMetadataProviders[0].GetMetadataAsync(null).ReturnsForAnyArgs(new(string key, string value)[]
            {
                ("key1", "value1"),
                ("key2", "value2")
            });

            eventMetadataProviders[1].GetMetadataAsync(null).ReturnsForAnyArgs(new(string key, string value)[]
            {
                ("key2", "value2-2")
            });

            IEvent @event = new Event1();
            IEventMessageDraft message = await sut.CreateMessageAsync(@event);

            message.Metadata.Should().HaveCount(2);
            message.Metadata.Should().Contain(x => x.Key == "key1" && x.Value == "value1");
            message.Metadata.Should().Contain(x => x.Key == "key2" && x.Value == "value2-2");
        }

        public class Event1 : IEvent
        {
        }
    }
}
