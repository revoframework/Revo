using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Extensions.Notifications.Channels.Buffering;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Bufferring
{
    public class BufferedAdaptorNotificationChannelTests
    {
        private readonly BufferedAdaptorNotificationChannel<Notification1> sut;
        private readonly IBufferedNotificationChannel bufferedChannel1;
        private readonly IBufferedNotificationChannel bufferedChannel2;

        public BufferedAdaptorNotificationChannelTests()
        {
            bufferedChannel1 = Substitute.For<IBufferedNotificationChannel>();
            bufferedChannel2 = Substitute.For<IBufferedNotificationChannel>();

            sut = new BufferedAdaptorNotificationChannel<Notification1>(new[] {bufferedChannel1, bufferedChannel2});
        }

        [Fact]
        public void NotificationTypes_ReturnsCorrectType()
        {
            Assert.Equal(1, sut.NotificationTypes.Count());
            Assert.Equal(typeof(Notification1), sut.NotificationTypes.First());
        }

        [Fact]
        public async Task SendNotificationAsync_SendsToAllBufferedChannels()
        {
            Notification1 n1 = new Notification1();
            await sut.PushNotificationAsync(n1);

            bufferedChannel1.Received(1).SendNotificationsAsync(
                Arg.Is<IReadOnlyCollection<INotification>>(x => x.Count() == 1 && x.First() == n1));
            bufferedChannel2.Received(1).SendNotificationsAsync(
                Arg.Is<IReadOnlyCollection<INotification>>(x => x.Count() == 1 && x.First() == n1));
        }

        public class Notification1 : INotification
        {
        }
    }
}
