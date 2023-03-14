using System.Threading.Tasks;
using NSubstitute;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Testing.Core;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Bufferring
{
    public class BufferingNotificationChannelTests
    {
        private readonly BufferingNotificationChannel<Notification1> sut;
        private readonly IBufferGovernor bufferGovernor;
        private readonly IBufferSelector<Notification1> bufferSelector;
        private readonly INotificationPipeline notificationPipeline;
        private readonly INotificationSerializer notificationSerializer;
        private readonly IBufferedNotificationStore bufferedNotificationStore;

        public BufferingNotificationChannelTests()
        {
            bufferGovernor = Substitute.For<IBufferGovernor>();
            bufferGovernor.Name.Returns("governor1");
            bufferSelector = Substitute.For<IBufferSelector<Notification1>>();
            notificationPipeline = Substitute.For<INotificationPipeline>();
            notificationPipeline.Name.Returns("pipeline1");
            notificationSerializer = Substitute.For<INotificationSerializer>();
            bufferedNotificationStore = Substitute.For<IBufferedNotificationStore>();

            FakeClock.Setup();

            sut = new BufferingNotificationChannel<Notification1>(bufferGovernor,
                bufferSelector, notificationPipeline, notificationSerializer,
                bufferedNotificationStore);
        }

        [Fact]
        public async Task SendNotificationAsync_SavesNotificationToBuffer()
        {
            Notification1 n1 = new Notification1();
            SerializedNotification serializedNotification = new SerializedNotification()
            {
                NotificationClassName = "Notification1",
                NotificationJson = "{}"
            };

            notificationSerializer.ToJson(n1).Returns(serializedNotification);
            bufferSelector.SelectBufferIdAsync(n1).Returns("buffer1");

            await sut.PushNotificationAsync(n1);

            bufferedNotificationStore.Received(1)
                .Add(serializedNotification, "buffer1", FakeClock.Now, bufferGovernor.Name,
                    notificationPipeline.Name);
        }

        public class Notification1 : INotification
        {
        }
    }
}
