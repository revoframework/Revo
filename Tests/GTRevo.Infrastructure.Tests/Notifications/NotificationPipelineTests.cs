using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Notifications;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Notifications
{
    public class NotificationPipelineTests
    {
        private readonly Guid pipelineId = Guid.NewGuid();
        private readonly IBufferedNotificationChannel channel1;
        private readonly IBufferedNotificationChannel channel2;
        private readonly NotificationPipeline sut;

        public NotificationPipelineTests()
        {
            channel1 = Substitute.For<IBufferedNotificationChannel>();
            channel2 = Substitute.For<IBufferedNotificationChannel>();
            sut = new NotificationPipeline(pipelineId, new[] {channel1, channel2});
        }

        [Fact]
        public void Id_ReturnsCorrectValue()
        {
            Assert.Equal(pipelineId, sut.Id);
        }

        [Fact]
        public async Task ProcessNotificationsAsync_SendsToChannels()
        {
            IEnumerable<INotification> notifications = new[] { new Notification1(), new Notification1() };
            await sut.ProcessNotificationsAsync(notifications);

            channel1.Received(1).SendNotificationsAsync(notifications);
            channel2.Received(1).SendNotificationsAsync(notifications);
        }

        public class Notification1 : INotification
        {
        }
    }
}
