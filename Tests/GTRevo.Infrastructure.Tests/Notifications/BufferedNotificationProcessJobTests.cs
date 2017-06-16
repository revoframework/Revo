using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTRevo.Infrastructure.Notifications;
using GTRevo.Infrastructure.Notifications.Model;
using GTRevo.Testing.DataAccess.EF6;
using NSubstitute;
using Xunit;

namespace GTRevo.Infrastructure.Tests.Notifications
{
    public class BufferedNotificationProcessJobTests
    {
        private readonly BufferedNotificationProcessJob sut;
        private readonly IBufferGovernor bufferGovernor1;
        private readonly FakeCrudRepository fakeCrudRepository;
        private readonly INotificationSerializer notificationSerializer;
        private readonly INotificationPipeline notificationPipeline1;
        private readonly INotificationPipeline notificationPipeline2;

        public BufferedNotificationProcessJobTests()
        {
            bufferGovernor1 = Substitute.For<IBufferGovernor>();
            bufferGovernor1.Id.Returns(Guid.NewGuid());
            fakeCrudRepository = new FakeCrudRepository();
            notificationSerializer = Substitute.For<INotificationSerializer>();
            notificationPipeline1 = Substitute.For<INotificationPipeline>();
            notificationPipeline1.Id.Returns(Guid.NewGuid());
            notificationPipeline2 = Substitute.For<INotificationPipeline>();
            notificationPipeline2.Id.Returns(Guid.NewGuid());

            notificationSerializer.FromJson(null).ReturnsForAnyArgs(ci =>
                new TestNotification()
                {
                    Data = ci.ArgAt<SerializedNotification>(0).NotificationJson,
                    TypeName = ci.ArgAt<SerializedNotification>(0).NotificationClassName
                });

            sut = new BufferedNotificationProcessJob(new[] {bufferGovernor1}, fakeCrudRepository,
                new[] {notificationPipeline1, notificationPipeline2}, notificationSerializer);
        }

        [Fact]
        public async Task Run_ProcessesNotificationsWithPipeline()
        {
            var notificationsToRelease = new MultiValueDictionary<NotificationBuffer, BufferedNotification>();

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), bufferGovernor1.Id,
                notificationPipeline1.Id);
            fakeCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, DateTime.Today);
            notificationsToRelease.Add(buffer1, notification1);
            fakeCrudRepository.Attach(notification1);
            BufferedNotification notification2 = new BufferedNotification(Guid.NewGuid(), "Notification2", "{}",
                buffer1, DateTime.Today);
            notificationsToRelease.Add(buffer1, notification2);
            fakeCrudRepository.Attach(notification2);

            NotificationBuffer buffer2 = new NotificationBuffer(Guid.NewGuid(), bufferGovernor1.Id,
                notificationPipeline2.Id);
            fakeCrudRepository.Attach(buffer2);
            BufferedNotification notification3 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer2, DateTime.Today);
            notificationsToRelease.Add(buffer2, notification3);
            fakeCrudRepository.Attach(notification3);
            
            bufferGovernor1.SelectNotificationsForReleaseAsync(fakeCrudRepository)
                .Returns(notificationsToRelease);

            await sut.Run();

            notificationPipeline1.ProcessNotificationsAsync(
                Arg.Is<IEnumerable<INotification>>(
                    x => x.Count() == 2
                         && x.Count(y => ((TestNotification) y).TypeName == "Notification1") == 1
                         && x.Count(y => ((TestNotification) y).TypeName == "Notification2") == 1));

            notificationPipeline2.ProcessNotificationsAsync(
                Arg.Is<IEnumerable<INotification>>(
                    x => x.Count() == 1
                         && x.Count(y => ((TestNotification)y).TypeName == "Notification3") == 1));
        }
        
        [Fact]
        public async Task Run_RemovesNotificationFromRepository()
        {
            var notificationsToRelease = new MultiValueDictionary<NotificationBuffer, BufferedNotification>();

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), bufferGovernor1.Id,
                notificationPipeline1.Id);
            fakeCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, DateTime.Today);
            notificationsToRelease.Add(buffer1, notification1);
            fakeCrudRepository.Attach(notification1);
            
            bufferGovernor1.SelectNotificationsForReleaseAsync(fakeCrudRepository)
                .Returns(notificationsToRelease);

            await sut.Run();

            Assert.DoesNotContain(notification1, fakeCrudRepository.FindAll<BufferedNotification>());
        }

        public class TestNotification : INotification
        {
            public string Data { get; set; }
            public string TypeName { get; set; }
        }
    }
}
