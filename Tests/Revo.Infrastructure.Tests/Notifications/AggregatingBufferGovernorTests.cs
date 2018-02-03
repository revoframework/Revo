using System;
using System.Linq;
using System.Threading.Tasks;
using Revo.Infrastructure.Notifications;
using Revo.Infrastructure.Notifications.Model;
using Revo.Testing.Core;
using Revo.Testing.DataAccess;
using Xunit;

namespace Revo.Infrastructure.Tests.Notifications
{
    public class AggregatingBufferGovernorTests
    {
        private readonly AggregatingBufferGovernor sut;
        private readonly Guid governorId = Guid.NewGuid();
        private readonly FakeCrudRepository fakeCrudRepository;

        public AggregatingBufferGovernorTests()
        {
            sut = new AggregatingBufferGovernor(governorId, TimeSpan.FromMinutes(5));
            fakeCrudRepository = new FakeCrudRepository();
        }

        [Fact]
        public void Id_ReturnsCorrectValue()
        {
            Assert.Equal(governorId, sut.Id);
        }

        [Fact]
        public async Task SelectNotificationsForReleaseAsync_ReturnsAllExpiredNotifications()
        {
            FakeClock.Setup();
            FakeClock.Now = DateTime.Now;

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), governorId, Guid.NewGuid());
            fakeCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, FakeClock.Now.Subtract(TimeSpan.FromMinutes(6)));
            fakeCrudRepository.Attach(notification1);
            BufferedNotification notification2 = new BufferedNotification(Guid.NewGuid(), "Notification2", "{}",
                buffer1, FakeClock.Now.Subtract(TimeSpan.FromMinutes(3)));
            fakeCrudRepository.Attach(notification2);

            NotificationBuffer buffer2 = new NotificationBuffer(Guid.NewGuid(), governorId, Guid.NewGuid());
            fakeCrudRepository.Attach(buffer2);
            BufferedNotification notification3 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer2, FakeClock.Now.Subtract(TimeSpan.FromMinutes(8)));
            fakeCrudRepository.Attach(notification3);
            
            NotificationBuffer buffer3 = new NotificationBuffer(Guid.NewGuid(), governorId, Guid.NewGuid());
            fakeCrudRepository.Attach(buffer3);
            BufferedNotification notification4 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer3, FakeClock.Now.Subtract(TimeSpan.FromMinutes(1)));
            fakeCrudRepository.Attach(notification4);

            var notifications = await sut.SelectNotificationsForReleaseAsync(fakeCrudRepository);

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

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), governorId, Guid.NewGuid());
            fakeCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, FakeClock.Now.Subtract(TimeSpan.FromMinutes(6)));
            fakeCrudRepository.Attach(notification1);

            NotificationBuffer buffer2 = new NotificationBuffer(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            fakeCrudRepository.Attach(buffer2);
            BufferedNotification notification3 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer2, FakeClock.Now.Subtract(TimeSpan.FromMinutes(8)));
            fakeCrudRepository.Attach(notification3);

            var notifications = await sut.SelectNotificationsForReleaseAsync(fakeCrudRepository);

            Assert.Equal(1, notifications.Keys.Count());
            Assert.Contains(buffer1, notifications.Keys);
        }
    }
}
