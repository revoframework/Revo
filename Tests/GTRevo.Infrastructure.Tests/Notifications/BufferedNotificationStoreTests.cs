using System;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Core.Transactions;
using GTRevo.Infrastructure.Notifications;
using GTRevo.Infrastructure.Notifications.Model;
using GTRevo.Testing.DataAccess;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Notifications
{
    public class BufferedNotificationStoreTests
    {
        private readonly BufferedNotificationStore sut;
        private readonly FakeCrudRepository fakeCrudRepository;

        public BufferedNotificationStoreTests()
        {
            fakeCrudRepository = Substitute.ForPartsOf<FakeCrudRepository>();
            sut = new BufferedNotificationStore(fakeCrudRepository);
        }

        [Fact]
        public async Task Add_AddsToRepository()
        {
            NotificationBuffer buffer = new NotificationBuffer(Guid.NewGuid(), Guid.NewGuid(),
                Guid.NewGuid());
            fakeCrudRepository.Attach(buffer);

            DateTimeOffset now = DateTimeOffset.Now;

            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                buffer.Id, now, Guid.NewGuid(), Guid.NewGuid());

            Assert.Equal(1, fakeCrudRepository.FindAllWithAdded<BufferedNotification>().Count());
            Assert.Single(fakeCrudRepository.FindAllWithAdded<BufferedNotification>(),
                x => x.NotificationClassName == "Notification1"
                     && x.NotificationJson == "{}"
                     && x.Buffer == buffer
                     && x.TimeQueued == now
                     && x.Id != Guid.Empty);
            Assert.Equal(1, fakeCrudRepository.FindAll<NotificationBuffer>().Count());
        }

        [Fact]
        public async Task SendNotificationAsync_CreatesNewBuffer()
        {
            Guid bufferId = Guid.NewGuid();
            Guid bufferGovernorId = Guid.NewGuid();
            Guid notificationPipelineId = Guid.NewGuid();

            await sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                bufferId, DateTime.Now, bufferGovernorId, notificationPipelineId);

            Assert.Equal(1, fakeCrudRepository.FindAllWithAdded<NotificationBuffer>().Count());
            Assert.Single(fakeCrudRepository.FindAllWithAdded<NotificationBuffer>(),
                x => x.Id == bufferId
                && x.GovernorId == bufferGovernorId
                && x.PipelineId == notificationPipelineId);
            Assert.Equal(1, fakeCrudRepository.FindAllWithAdded<BufferedNotification>().Count());
            Assert.Single(fakeCrudRepository.FindAllWithAdded<BufferedNotification>(),
                x => x.Buffer == fakeCrudRepository.FindAllWithAdded<NotificationBuffer>().First());
        }

        [Fact]
        public async Task OnTransactionSucceededAsync_SavesRepository()
        {
            sut.Add(new SerializedNotification() { NotificationClassName = "Notification1", NotificationJson = "{}" },
                Guid.NewGuid(), DateTimeOffset.Now, Guid.NewGuid(), Guid.NewGuid());
            await sut.CommitAsync();

            fakeCrudRepository.Received(1).SaveChangesAsync();
        }
    }
}
