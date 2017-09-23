using System;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Notifications;
using GTRevo.Testing.Core;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Notifications
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
            bufferGovernor.Id.Returns(Guid.NewGuid());
            bufferSelector = Substitute.For<IBufferSelector<Notification1>>();
            notificationPipeline = Substitute.For<INotificationPipeline>();
            notificationPipeline.Id.Returns(Guid.NewGuid());
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
            Guid bufferId = Guid.NewGuid();
            SerializedNotification serializedNotification = new SerializedNotification()
            {
                NotificationClassName = "Notification1",
                NotificationJson = "{}"
            };

            notificationSerializer.ToJson(n1).Returns(serializedNotification);
            bufferSelector.SelectBufferIdAsync(n1).Returns(bufferId);

            await sut.SendNotificationAsync(n1);

            bufferedNotificationStore.Received(1)
                .Add(serializedNotification, bufferId, FakeClock.Now, bufferGovernor.Id,
                    notificationPipeline.Id);
        }

        public class Notification1 : INotification
        {
        }
    }
}
