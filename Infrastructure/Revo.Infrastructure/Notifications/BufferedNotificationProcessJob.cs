using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Infrastructure.Notifications.Model;

namespace Revo.Infrastructure.Notifications
{
    public class BufferedNotificationProcessJob
    {
        private readonly IBufferGovernor[] bufferGovernors;
        private readonly ICrudRepository crudRepository;
        private readonly INotificationPipeline[] notificationPipelines;
        private readonly INotificationSerializer notificationSerializer;

        public BufferedNotificationProcessJob(IBufferGovernor[] bufferGovernors,
            ICrudRepository crudRepository,
            INotificationPipeline[] notificationPipelines,
            INotificationSerializer notificationSerializer)
        {
            this.bufferGovernors = bufferGovernors;
            this.crudRepository = crudRepository;
            this.notificationPipelines = notificationPipelines;
            this.notificationSerializer = notificationSerializer;
        }

        public async Task Run()
        {
            try
            {
                foreach (IBufferGovernor bufferGovernor in bufferGovernors)
                {
                    MultiValueDictionary<NotificationBuffer, BufferedNotification> toRelease = await bufferGovernor
                        .SelectNotificationsForReleaseAsync(crudRepository);

                    foreach (var bufferPair in toRelease)
                    {
                        INotificationPipeline pipeline =
                            notificationPipelines.FirstOrDefault(x => x.Id == bufferPair.Key.PipelineId);
                        if (pipeline == null)
                        {
                            throw new InvalidOperationException(
                                $"Notification pipeline not found: {bufferPair.Key.PipelineId}");
                        }

                        IEnumerable<INotification> notifications = bufferPair.Value
                            .Select(
                                x =>
                                    notificationSerializer.FromJson(new SerializedNotification()
                                    {
                                        NotificationJson = x.NotificationJson,
                                        NotificationClassName = x.NotificationClassName
                                    }));

                        await pipeline.ProcessNotificationsAsync(notifications);

                        foreach (BufferedNotification buffNotification in bufferPair.Value)
                        {
                            crudRepository.Remove(buffNotification);
                        }
                    }
                }
            }
            finally
            {
                await crudRepository.SaveChangesAsync();
            }
        }
    }
}
