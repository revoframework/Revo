using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Core.Transactions;
using Revo.Infrastructure.Notifications;
using Revo.Infrastructure.Notifications.Model;
using NSubstitute;
using Revo.DataAccess.InMemory;
using Xunit;

namespace Revo.Infrastructure.Tests.Notifications
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
            NotificationBuffer buffer = new NotificationBuffer(Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid());
            inMemoryCrudRepository.Attach(buffer);

            DateTimeOffset now = DateTimeOffset.Now;

            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                buffer.Id, now, Guid.NewGuid(), Guid.NewGuid());

            Assert.Equal(1, inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>().Count());
            Assert.Single(inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>(),
                x => x.NotificationClassName == "Notification1"
                     && x.NotificationJson == "{}"
                     && x.Buffer == buffer
                     && x.TimeQueued == now
                     && x.Id != Guid.Empty);
            Assert.Equal(1, inMemoryCrudRepository.FindAll<NotificationBuffer>().Count());
        }

        [Fact]
        public async Task SendNotificationAsync_CreatesNewBuffer()
        {
            Guid bufferId = Guid.NewGuid();
            Guid bufferGovernorId = Guid.NewGuid();
            Guid notificationPipelineId = Guid.NewGuid();

            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                bufferId, DateTime.Now, bufferGovernorId, notificationPipelineId);

            Assert.Equal(1, inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>().Count());
            Assert.Single(inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>(),
                x => x.Id == bufferId
                && x.GovernorId == bufferGovernorId
                && x.PipelineId == notificationPipelineId);
            Assert.Equal(1, inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>().Count());
            Assert.Single(inMemoryCrudRepository.FindAllWithAdded<BufferedNotification>(),
                x => x.Buffer == inMemoryCrudRepository.FindAllWithAdded<NotificationBuffer>().First());
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_SavesRepository()
        {
            sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                Guid.NewGuid(), DateTimeOffset.Now, Guid.NewGuid(), Guid.NewGuid());
            await sut.CommitAsync();

            inMemoryCrudRepository.Received(1).SaveChangesAsync();
        }
    }
}
