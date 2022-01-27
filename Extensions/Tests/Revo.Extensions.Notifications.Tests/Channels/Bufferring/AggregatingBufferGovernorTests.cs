using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.InMemory;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Extensions.Notifications.Model;
using Revo.Testing.Core;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Bufferring
{
    public class AggregatingBufferGovernorTests
    {
        private readonly AggregatingBufferGovernor sut;
        private readonly string governorName = "governor1";
        private readonly InMemoryCrudRepository inMemoryCrudRepository;

        public AggregatingBufferGovernorTests()
        {
            sut = new AggregatingBufferGovernor(governorName, TimeSpan.FromMinutes(5));
            inMemoryCrudRepository = new InMemoryCrudRepository();
        }

        [Fact]
        public void Id_ReturnsCorrectValue()
        {
            Assert.Equal(governorName, sut.Name);
        }

        [Fact]
        public async Task SelectNotificationsForReleaseAsync_ReturnsAllExpiredNotifications()
        {
            FakeClock.Setup();
            FakeClock.Now = DateTime.Now;

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), "buffer1", governorName, "pipeline1");
            inMemoryCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, FakeClock.Now.Subtract(TimeSpan.FromMinutes(6)));
            inMemoryCrudRepository.Attach(notification1);
            BufferedNotification notification2 = new BufferedNotification(Guid.NewGuid(), "Notification2", "{}",
                buffer1, FakeClock.Now.Subtract(TimeSpan.FromMinutes(3)));
            inMemoryCrudRepository.Attach(notification2);
            buffer1.Notifications = new List<BufferedNotification>() { notification1, notification2 };

            NotificationBuffer buffer2 = new NotificationBuffer(Guid.NewGuid(), "buffer2", governorName, "pipeline2");
            inMemoryCrudRepository.Attach(buffer2);
            BufferedNotification notification3 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer2, FakeClock.Now.Subtract(TimeSpan.FromMinutes(8)));
            inMemoryCrudRepository.Attach(notification3);
            buffer2.Notifications = new List<BufferedNotification>() { notification3 };

            NotificationBuffer buffer3 = new NotificationBuffer(Guid.NewGuid(), "buffer3", governorName, "pipeline3");
            inMemoryCrudRepository.Attach(buffer3);
            BufferedNotification notification4 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer3, FakeClock.Now.Subtract(TimeSpan.FromMinutes(1)));
            inMemoryCrudRepository.Attach(notification4);
            buffer3.Notifications = new List<BufferedNotification>() { notification4 };

            var notifications = await sut.SelectNotificationsForReleaseAsync(inMemoryCrudRepository);

            Assert.Equal(2, notifications.Keys.Count());
            Assert.Equal(2, notifications[buffer1].Count);
            Assert.Contains(notification1, notifications[buffer1]);
            Assert.Contains(notification2, notifications[buffer1]);
            Assert.Equal(1, notifications[buffer2].Count);
            Assert.Contains(notification3, notifications[buffer2]);
        }

        [Fact]
        public async Task SelectNotificationsForReleaseAsync_DoesntSelectFromOtherGovernors()
        {
            FakeClock.Setup();
            FakeClock.Now = DateTime.Now;

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), "buffer1", governorName, "pipeline1");
            inMemoryCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, FakeClock.Now.Subtract(TimeSpan.FromMinutes(6)));
            inMemoryCrudRepository.Attach(notification1);
            buffer1.Notifications = new List<BufferedNotification>() { notification1 };

            NotificationBuffer buffer2 = new NotificationBuffer(Guid.NewGuid(), "buffer2", "governor2", "pipeline1");
            inMemoryCrudRepository.Attach(buffer2);
            BufferedNotification notification3 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer2, FakeClock.Now.Subtract(TimeSpan.FromMinutes(8)));
            inMemoryCrudRepository.Attach(notification3);
            buffer2.Notifications = new List<BufferedNotification>() { notification3 };

            var notifications = await sut.SelectNotificationsForReleaseAsync(inMemoryCrudRepository);

            Assert.Equal(1, notifications.Keys.Count());
            Assert.Contains(buffer1, notifications.Keys);
        }
    }
}
