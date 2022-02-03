using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Revo.DataAccess.Entities;
using Revo.Extensions.Notifications.Model;

namespace Revo.Extensions.Notifications.Channels.Buffering
{
    public class BufferedNotificationStore : IBufferedNotificationStore
    {
        private readonly ICrudRepository crudRepository;
        private readonly List<NotificationBuffer> uncommittedNewBuffers = new List<NotificationBuffer>();

        public BufferedNotificationStore(ICrudRepository crudRepository)
        {
            this.crudRepository = crudRepository;
        }
        
        public async Task CommitAsync()
        {
            await crudRepository.SaveChangesAsync();
            uncommittedNewBuffers.Clear();
        }

        public async Task Add(SerializedNotification serializedNotification, string bufferName,
            DateTimeOffset timeQueued, string bufferGovernorName, string notificationPipelineName)
        {
            NotificationBuffer buffer = uncommittedNewBuffers.FirstOrDefault(x => x.Name == bufferName)
                ?? await crudRepository.FirstOrDefaultAsync<NotificationBuffer>(x => x.Name == bufferName);
            if (buffer == null)
            {
                buffer = new NotificationBuffer(Guid.NewGuid(), bufferName, bufferGovernorName, notificationPipelineName);
                crudRepository.Add(buffer);
                uncommittedNewBuffers.Add(buffer);
            }

            BufferedNotification notification = new BufferedNotification(
                id: Guid.NewGuid(),
                buffer: buffer,
                notificationClassName: serializedNotification.NotificationClassName,
                notificationJson: serializedNotification.NotificationJson,
                timeQueued: timeQueued
            );

            crudRepository.Add(notification);
        }
    }
}
