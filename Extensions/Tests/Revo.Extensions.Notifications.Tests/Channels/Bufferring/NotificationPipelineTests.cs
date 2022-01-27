using NSubstitute;
using Revo.Extensions.Notifications.Channels.Buffering;
using System.Threading.Tasks;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Bufferring
{
    public class NotificationPipelineTests
    {
        private readonly IBufferedNotificationChannel channel1;
        private readonly IBufferedNotificationChannel channel2;
        private readonly NotificationPipeline sut;

        public NotificationPipelineTests()
        {
            channel1 = Substitute.For<IBufferedNotificationChannel>();
            channel2 = Substitute.For<IBufferedNotificationChannel>();
            sut = new NotificationPipeline("pipeline1", new[] {channel1, channel2});
        }

        [Fact]
        public void Name_ReturnsCorrectValue()
        {
            Assert.Equal("pipeline1", sut.Name);
        }

        [Fact]
        public async Task ProcessNotificationsAsync_SendsToChannels()
        {
            var notifications = new[] { new Notification1(), new Notification1() };
            await sut.ProcessNotificationsAsync(notifications);

            channel1.Received(1).SendNotificationsAsync(notifications);
            channel2.Received(1).SendNotificationsAsync(notifications);
        }

        public class Notification1 : INotification
        {
        }
    }
}
