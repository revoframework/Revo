using FluentAssertions;
using NSubstitute;
using Revo.DataAccess.InMemory;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Extensions.Notifications.Model;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Bufferring
{
    public class BufferedNotificationStoreTests
    {
        private readonly BufferedNotificationStore sut;
        private readonly InMemoryCrudRepository inMemoryCrudRepository;

        public BufferedNotificationStoreTests()
        {
            inMemoryCrudRepository = Substitute.ForPartsOf<InMemoryCrudRepository>();
            sut = new BufferedNotificationStore(inMemoryCrudRepository);
        }

        [Fact]
        public async Task Add_AddsToRepository()
        {
            NotificationBuffer buffer = new NotificationBuffer(Guid.NewGuid(), "buffer1",
                "governor1", "pipeline1");
            inMemoryCrudRepository.Attach(buffer);

            DateTimeOffset now = DateTimeOffset.Now;

            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                buffer.Name, now, "governor1", "pipeline1");

            inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>()
                .Should().HaveCount(1)
                .And.ContainSingle(x => x.NotificationClassName == "Notification1"
                                        && x.NotificationJson == "{}"
                                        && x.Buffer == buffer
                                        && x.TimeQueued == now
                                        && x.Id != Guid.Empty);
        }

        [Fact]
        public async Task SendNotificationAsync_CreatesNewBuffer()
        {
            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                "buffer1", DateTime.Now, "governor1", "pipeline1");

            inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>()
                .Should().HaveCount(1)
                .And.ContainSingle(x => x.Name == "buffer1"
                                                 && x.GovernorName == "governor1"
                                                 && x.PipelineName == "pipeline1");

            inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>()
                .Should().HaveCount(1);
            inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>()
                .Should().ContainSingle(x => x.Buffer == inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>().First());
        }

        [Fact]
        public async Task SendNotificationAsync_CreatesBufferOnce()
        {
            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                "buffer1", DateTime.Now, "governor1", "pipeline1");
            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                "buffer1", DateTime.Now, "governor1", "pipeline1");

            inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>()
                .Should().HaveCount(1)
                .And.ContainSingle(x => x.Name == "buffer1"
                                        && x.GovernorName == "governor1"
                                        && x.PipelineName == "pipeline1");

            inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>()
                .Should().HaveCount(2)
                .And.OnlyContain(x => x.Buffer == inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>().First());
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_SavesRepository()
        {
            sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                "buffer1", DateTimeOffset.Now, "governor1", "pipeline1");
            await sut.CommitAsync();

            inMemoryCrudRepository.Received(1).SaveChangesAsync();
        }
    }
}
