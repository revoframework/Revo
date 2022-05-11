using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Revo.Core.Collections;
using Revo.Core.Core;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Model;
using Revo.Infrastructure.Jobs;
using Revo.Infrastructure.Jobs.InMemory;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class ProcessBufferedNotificationsJobHandler : IJobHandler<ProcessBufferedNotificationsJob>
    {
        private readonly IBufferGovernor[] bufferGovernors;
        private readonly ICrudRepository crudRepository;
        private readonly INotificationPipeline[] notificationPipelines;
        private readonly INotificationSerializer notificationSerializer;
        private readonly IInMemoryJobScheduler inMemoryJobScheduler;

        public ProcessBufferedNotificationsJobHandler(IBufferGovernor[] bufferGovernors,
            ICrudRepository crudRepository,
            INotificationPipeline[] notificationPipelines,
            INotificationSerializer notificationSerializer,
            IInMemoryJobScheduler inMemoryJobScheduler)
        {
            this.bufferGovernors = bufferGovernors;
            this.crudRepository = crudRepository;
            this.notificationPipelines = notificationPipelines;
            this.notificationSerializer = notificationSerializer;
            this.inMemoryJobScheduler = inMemoryJobScheduler;
        }

        public async Task HandleAsync(ProcessBufferedNotificationsJob job, CancellationToken cancellationToken)
        {
            foreach (IBufferGovernor bufferGovernor in bufferGovernors)
            {
                MultiValueDictionary<NotificationBuffer, BufferedNotification> toRelease = await bufferGovernor
                    .SelectNotificationsForReleaseAsync(crudRepository);
                var byPipeline = toRelease.GroupBy(x => x.Key.PipelineName);

                foreach (var pipelineNotifications in byPipeline)
                {
                    INotificationPipeline pipeline =
                        notificationPipelines.FirstOrDefault(x => x.Name == pipelineNotifications.Key);
                    if (pipeline == null)
                    {
                        throw new InvalidOperationException(
                            $"Notification pipeline not found: {pipelineNotifications.Key}");
                    }

                    IReadOnlyCollection<INotification> notifications = pipelineNotifications
                        .SelectMany(x => x.Value)
                        .Select(
                            x =>
                                notificationSerializer.FromJson(new SerializedNotification()
                                {
                                    NotificationJson = x.NotificationJson,
                                    NotificationClassName = x.NotificationClassName
                                }))
                        .ToArray();

                    await pipeline.ProcessNotificationsAsync(notifications);

                    foreach (BufferedNotification buffNotification in pipelineNotifications.SelectMany(x => x.Value))
                    {
                        crudRepository.Remove(buffNotification);
                    }

                    await crudRepository.SaveChangesAsync();
                }
            }

            DateTimeOffset scheduledTime = job.ScheduledTime + TimeSpan.FromMinutes(1);
            if (scheduledTime <= Clock.Current.UtcNow)
            {
                await inMemoryJobScheduler.EnqeueJobAsync(new ProcessBufferedNotificationsJob(Clock.Current.UtcNow), null);
            }
            else
            {
                await inMemoryJobScheduler.ScheduleJobAsync(new ProcessBufferedNotificationsJob(scheduledTime), scheduledTime);
            }
        }
    }
}
