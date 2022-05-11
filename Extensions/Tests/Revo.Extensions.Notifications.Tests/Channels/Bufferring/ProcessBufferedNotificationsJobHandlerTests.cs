using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Revo.Core.Collections;
using Revo.Core.Core;
using Revo.DataAccess.InMemory;
using Revo.Extensions.Notifications.Channels.Buffering;
using Revo.Extensions.Notifications.Model;
using Revo.Infrastructure.Jobs.InMemory;
using Xunit;

namespace Revo.Extensions.Notifications.Tests.Channels.Bufferring
{
    public class ProcessBufferedNotificationsJobHandlerTests
    {
        private readonly ProcessBufferedNotificationsJobHandler sut;
        private readonly IBufferGovernor bufferGovernor1;
        private readonly InMemoryCrudRepository inMemoryCrudRepository;
        private readonly INotificationSerializer notificationSerializer;
        private readonly INotificationPipeline notificationPipeline1;
        private readonly INotificationPipeline notificationPipeline2;
        private readonly IInMemoryJobScheduler inMemoryJobScheduler;

        public ProcessBufferedNotificationsJobHandlerTests()
        {
            bufferGovernor1 = Substitute.For<IBufferGovernor>();
            bufferGovernor1.Name.Returns("governor1");
            inMemoryCrudRepository = new InMemoryCrudRepository();
            notificationSerializer = Substitute.For<INotificationSerializer>();
            notificationPipeline1 = Substitute.For<INotificationPipeline>();
            notificationPipeline1.Name.Returns("pipeline1");
            notificationPipeline2 = Substitute.For<INotificationPipeline>();
            notificationPipeline2.Name.Returns("pipeline2");

            inMemoryJobScheduler = Substitute.For<IInMemoryJobScheduler>();

            notificationSerializer.FromJson(null).ReturnsForAnyArgs(ci =>
                new TestNotification()
                {
                    Data = ci.ArgAt<SerializedNotification>(0).NotificationJson,
                    TypeName = ci.ArgAt<SerializedNotification>(0).NotificationClassName
                });

            sut = new ProcessBufferedNotificationsJobHandler(new[] {bufferGovernor1}, inMemoryCrudRepository,
                new[] {notificationPipeline1, notificationPipeline2}, notificationSerializer, inMemoryJobScheduler);
        }

        [Fact]
        public async Task HandleAsync_ProcessesNotificationsWithPipeline()
        {
            var notificationsToRelease = new MultiValueDictionary<NotificationBuffer, BufferedNotification>();

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), "buffer1", bufferGovernor1.Name,
                notificationPipeline1.Name);
            inMemoryCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, DateTime.Today);
            notificationsToRelease.Add(buffer1, notification1);
            inMemoryCrudRepository.Attach(notification1);
            BufferedNotification notification2 = new BufferedNotification(Guid.NewGuid(), "Notification2", "{}",
                buffer1, DateTime.Today);
            notificationsToRelease.Add(buffer1, notification2);
            inMemoryCrudRepository.Attach(notification2);

            NotificationBuffer buffer2 = new NotificationBuffer(Guid.NewGuid(), "buffer2", bufferGovernor1.Name,
                notificationPipeline2.Name);
            inMemoryCrudRepository.Attach(buffer2);
            BufferedNotification notification3 = new BufferedNotification(Guid.NewGuid(), "Notification3", "{}",
                buffer2, DateTime.Today);
            notificationsToRelease.Add(buffer2, notification3);
            inMemoryCrudRepository.Attach(notification3);
            
            bufferGovernor1.SelectNotificationsForReleaseAsync(inMemoryCrudRepository)
                .Returns(notificationsToRelease);

            var job = new ProcessBufferedNotificationsJob(Clock.Current.UtcNow);
            await sut.HandleAsync(job, CancellationToken.None);

            notificationPipeline1.Received(1).ProcessNotificationsAsync(
                Arg.Is<IReadOnlyCollection<INotification>>(
                    x => x.Count() == 2
                         && x.Count(y => ((TestNotification) y).TypeName == "Notification1") == 1
                         && x.Count(y => ((TestNotification) y).TypeName == "Notification2") == 1));

            notificationPipeline2.Received(1).ProcessNotificationsAsync(
                Arg.Is<IReadOnlyCollection<INotification>>(
                    x => x.Count() == 1
                         && x.Count(y => ((TestNotification)y).TypeName == "Notification3") == 1));
        }
        
        [Fact]
        public async Task HandleAsync_RemovesNotificationFromRepository()
        {
            var notificationsToRelease = new MultiValueDictionary<NotificationBuffer, BufferedNotification>();

            NotificationBuffer buffer1 = new NotificationBuffer(Guid.NewGuid(), "buffer1", bufferGovernor1.Name,
                notificationPipeline1.Name);
            inMemoryCrudRepository.Attach(buffer1);
            BufferedNotification notification1 = new BufferedNotification(Guid.NewGuid(), "Notification1", "{}",
                buffer1, DateTime.Today);
            notificationsToRelease.Add(buffer1, notification1);
            inMemoryCrudRepository.Attach(notification1);
            
            bufferGovernor1.SelectNotificationsForReleaseAsync(inMemoryCrudRepository)
                .Returns(notificationsToRelease);

            var job = new ProcessBufferedNotificationsJob(Clock.Current.UtcNow);
            await sut.HandleAsync(job, CancellationToken.None);

            Assert.DoesNotContain(notification1, inMemoryCrudRepository.FindAll<BufferedNotification>());
        }

        public class TestNotification : INotification
        {
            public string Data { get; set; }
            public string TypeName { get; set; }
        }
    }
}
